using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<PaginatedList<OrderSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<OrderSummaryDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            return Result<PaginatedList<OrderSummaryDto>>.Failure("Customer not found");

        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var orders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.CreatedAt,
                o.Subtotal,
                o.TaxAmount,
                o.DiscountAmount,
                o.TotalAmount,
                o.Status.ToString(),
                o.OrderItems.Count
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<OrderSummaryDto>.Create(
            orders,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<OrderSummaryDto>>.Success(paginatedList);
    }
}
