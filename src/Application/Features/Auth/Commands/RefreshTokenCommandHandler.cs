using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find user by refresh token
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Store)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null)
            return Result<RefreshTokenResponse>.Failure("Invalid refresh token");

        // Check if refresh token is expired
        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return Result<RefreshTokenResponse>.Failure("Refresh token has expired");

        // Check if user is active
        if (!user.IsActive)
            return Result<RefreshTokenResponse>.Failure("User account is inactive");

        // Get user roles
        var roles = new List<string> { user.Role.Name };

        // Generate new tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Update refresh token
        user.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        await _context.SaveChangesAsync(cancellationToken);

        // Build response
        var response = new RefreshTokenResponse(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15)
        );

        return Result<RefreshTokenResponse>.Success(response);
    }
}
