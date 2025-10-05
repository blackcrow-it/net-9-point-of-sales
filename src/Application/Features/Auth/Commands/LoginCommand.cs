using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LoginCommand(
    string Username,
    string Password,
    Guid? StoreId = null
) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    Guid? StoreId,
    List<string> Roles
);
