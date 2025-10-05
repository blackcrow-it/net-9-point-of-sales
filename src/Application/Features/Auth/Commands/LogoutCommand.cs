using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LogoutCommand(
    Guid UserId
) : IRequest<Result>;
