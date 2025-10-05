using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Commands;

/// <summary>
/// Command to create new role with permissions
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description,
    List<Guid> PermissionIds,
    bool IsActive = true
) : IRequest<Result<Guid>>;
