using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Employees.Queries;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PaginatedList<EmployeeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.Store)
            .AsQueryable();

        // Apply filters
        if (request.StoreId.HasValue)
            query = query.Where(u => u.StoreId == request.StoreId.Value);

        if (request.RoleId.HasValue)
            query = query.Where(u => u.RoleId == request.RoleId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var employees = await query
            .OrderBy(u => u.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new EmployeeDto(
                u.Id,
                u.Username,
                u.FullName,
                u.Email,
                u.Phone,
                u.StoreId,
                u.Store != null ? u.Store.Name : null,
                u.RoleId,
                u.Role.Name,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<EmployeeDto>.Create(
            employees,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<EmployeeDto>>.Success(paginatedList);
    }
}
