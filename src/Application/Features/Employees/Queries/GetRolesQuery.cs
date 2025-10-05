using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Queries;

/// <summary>
/// Query to retrieve roles with permissions
/// </summary>
public record GetRolesQuery(
    bool? IsActive = null
) : IRequest<Result<List<RoleDto>>>;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    List<PermissionDto> Permissions
);

public record PermissionDto(
    Guid Id,
    string Resource,
    string Action
);
