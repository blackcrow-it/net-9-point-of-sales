using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Employees.Queries;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<List<RoleDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Roles
            .AsNoTracking()
            .Include(r => r.Permissions)
            .AsQueryable();

        // Apply active filter if specified
        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.IsActive,
                r.Permissions.Select(p => new PermissionDto(
                    p.Id,
                    p.Resource,
                    p.Action
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<RoleDto>>.Success(roles);
    }
}
