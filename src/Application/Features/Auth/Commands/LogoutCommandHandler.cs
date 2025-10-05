using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IRedisCacheService _cacheService;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        IRedisCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;

        await _context.SaveChangesAsync(cancellationToken);

        // Clear Redis session cache
        await _cacheService.RemoveAsync($"user_session:{request.UserId}", cancellationToken);

        return Result.Success();
    }
}
