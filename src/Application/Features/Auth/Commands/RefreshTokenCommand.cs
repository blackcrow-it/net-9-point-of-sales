using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
