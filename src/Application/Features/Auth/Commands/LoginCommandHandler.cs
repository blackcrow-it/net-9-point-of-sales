using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by username
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Store)
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null)
            return Result<LoginResponse>.Failure("Invalid username or password");

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid username or password");

        // Check if user is active
        if (!user.IsActive)
            return Result<LoginResponse>.Failure("User account is inactive");

        // Validate store if provided
        if (request.StoreId.HasValue && user.StoreId != request.StoreId)
            return Result<LoginResponse>.Failure("User is not assigned to this store");

        // Get user roles
        var roles = new List<string> { user.Role.Name };

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Update user with refresh token
        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        user.MarkLastLogin();

        await _context.SaveChangesAsync(cancellationToken);

        // Build response
        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.Email ?? string.Empty,
            user.StoreId,
            roles
        );

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15), // 15-minute expiration
            userDto
        );

        return Result<LoginResponse>.Success(response);
    }
}
