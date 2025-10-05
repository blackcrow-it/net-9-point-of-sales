using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries;

public class TopProductsReportQueryHandler : IRequestHandler<TopProductsReportQuery, Result<List<TopProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public TopProductsReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TopProductDto>>> Handle(TopProductsReportQuery request, CancellationToken cancellationToken)
    {
        // Verify store exists
        var storeExists = await _context.Stores
            .AnyAsync(s => s.Id == request.StoreId, cancellationToken);

        if (!storeExists)
        {
            return Result<List<TopProductDto>>.Failure("Store not found");
        }

        // Get top products by revenue
        var topProducts = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Order)
            .Include(oi => oi.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .Where(oi => oi.Order.StoreId == request.StoreId
                && oi.Order.CreatedAt >= request.DateFrom
                && oi.Order.CreatedAt <= request.DateTo
                && oi.Order.Status == OrderStatus.Completed)
            .GroupBy(oi => new
            {
                ProductId = oi.ProductVariant.ProductId,
                ProductName = oi.ProductVariant.Product.Name,
                ProductSKU = oi.ProductVariant.Product.SKU
            })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.ProductSKU,
                (int)g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.LineTotal),
                g.Select(oi => oi.OrderId).Distinct().Count()
            ))
            .OrderByDescending(p => p.TotalRevenue)
            .Take(request.TopN)
            .ToListAsync(cancellationToken);

        return Result<List<TopProductDto>>.Success(topProducts);
    }
}
