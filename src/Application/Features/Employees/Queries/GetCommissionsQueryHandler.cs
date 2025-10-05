using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Employees.Queries;

public class GetCommissionsQueryHandler : IRequestHandler<GetCommissionsQuery, Result<PaginatedList<CommissionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCommissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<CommissionDto>>> Handle(GetCommissionsQuery request, CancellationToken cancellationToken)
    {
        // Verify employee exists
        var employeeExists = await _context.Users
            .AnyAsync(u => u.Id == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            return Result<PaginatedList<CommissionDto>>.Failure("Employee not found");
        }

        // Build query with filters
        var query = _context.Commissions
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Order)
            .Where(c => c.UserId == request.EmployeeId);

        if (request.StartDate.HasValue)
        {
            query = query.Where(c => c.CalculatedDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(c => c.CalculatedDate <= request.EndDate.Value);
        }

        query = query.OrderByDescending(c => c.CalculatedDate);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var commissions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CommissionDto(
                c.Id,
                c.UserId,
                c.User.FullName,
                c.OrderId,
                c.Order.OrderNumber,
                c.OrderAmount,
                c.CommissionRate,
                c.CommissionAmount,
                c.CalculatedDate,
                c.Status.ToString()
            ))
            .ToListAsync(cancellationToken);

        var result = new PaginatedList<CommissionDto>(
            commissions,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<CommissionDto>>.Success(result);
    }
}
