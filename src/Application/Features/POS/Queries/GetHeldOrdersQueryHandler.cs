using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Queries;

public class GetHeldOrdersQueryHandler : IRequestHandler<GetHeldOrdersQuery, Result<List<OrderSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetHeldOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<OrderSummaryDto>>> Handle(GetHeldOrdersQuery request, CancellationToken cancellationToken)
    {
        var heldOrders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.StoreId == request.StoreId && o.Status == OrderStatus.OnHold)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = heldOrders.Select(o => new OrderSummaryDto(
            o.Id,
            o.OrderNumber,
            o.CustomerId,
            o.Customer?.Name,
            o.TotalAmount,
            o.OrderItems.Count,
            o.CreatedAt,
            o.Status.ToString()
        )).ToList();

        return Result<List<OrderSummaryDto>>.Success(result);
    }
}
