using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Customers.Queries;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PaginatedList<CustomerDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.CustomerGroup)
            .AsQueryable();

        // Apply filters
        if (request.CustomerGroupId.HasValue)
            query = query.Where(c => c.CustomerGroupId == request.CustomerGroupId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerDto(
                c.Id,
                c.CustomerNumber,
                c.Name,
                c.Phone,
                c.Email,
                c.CustomerGroupId,
                c.CustomerGroup != null ? c.CustomerGroup.Name : null,
                c.Address,
                c.LoyaltyPoints,
                c.TotalSpent,
                c.TotalOrders,
                c.LastPurchaseDate,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<CustomerDto>.Create(
            customers,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<CustomerDto>>.Success(paginatedList);
    }
}
