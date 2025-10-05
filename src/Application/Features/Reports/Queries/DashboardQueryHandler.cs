using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Reports.Queries;

public class DashboardQueryHandler : IRequestHandler<DashboardQuery, Result<DashboardDto>>
{
    private readonly IApplicationDbContext _context;

    public DashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardDto>> Handle(DashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // Today's metrics
        var todayOrders = await _context.Orders
            .Where(o => o.StoreId == request.StoreId 
                     && o.CreatedAt >= todayStart 
                     && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var todaySales = todayOrders.Sum(o => o.TotalAmount);
        var todayOrderCount = todayOrders.Count;

        // Week's metrics
        var weekOrders = await _context.Orders
            .Where(o => o.StoreId == request.StoreId 
                     && o.CreatedAt >= weekStart 
                     && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var weekSales = weekOrders.Sum(o => o.TotalAmount);
        var weekOrderCount = weekOrders.Count;

        // Month's metrics
        var monthOrders = await _context.Orders
            .Where(o => o.StoreId == request.StoreId 
                     && o.CreatedAt >= monthStart 
                     && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var monthSales = monthOrders.Sum(o => o.TotalAmount);
        var monthOrderCount = monthOrders.Count;

        // Active customers (purchased in last 30 days)
        var activeCustomers = await _context.Orders
            .Where(o => o.StoreId == request.StoreId 
                     && o.CreatedAt >= now.AddDays(-30) 
                     && o.CustomerId.HasValue
                     && o.Status == OrderStatus.Completed)
            .Select(o => o.CustomerId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Low stock count
        var lowStockCount = await _context.InventoryLevels
            .Include(il => il.ProductVariant)
                .ThenInclude(pv => pv.Product)
            .Where(il => il.StoreId == request.StoreId 
                      && il.ProductVariant.Product.TrackInventory
                      && il.ProductVariant.Product.ReorderLevel.HasValue
                      && il.AvailableQuantity <= il.ProductVariant.Product.ReorderLevel.Value)
            .CountAsync(cancellationToken);

        // Top selling products (last 7 days)
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.StoreId == request.StoreId 
                      && oi.Order.CreatedAt >= now.AddDays(-7)
                      && oi.Order.Status == OrderStatus.Completed)
            .GroupBy(oi => new { oi.ProductVariantId, oi.ProductName })
            .Select(g => new TopSellingProductDto(
                g.Key.ProductVariantId,
                g.Key.ProductName,
                g.Sum(oi => oi.LineTotal),
                g.Sum(oi => oi.Quantity)
            ))
            .OrderByDescending(p => p.TotalRevenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        var dashboard = new DashboardDto(
            TodaySales: todaySales,
            TodayOrders: todayOrderCount,
            ActiveCustomers: activeCustomers,
            LowStockCount: lowStockCount,
            WeekSales: weekSales,
            WeekOrders: weekOrderCount,
            MonthSales: monthSales,
            MonthOrders: monthOrderCount,
            TopSellingProducts: topProducts,
            GeneratedAt: now
        );

        return Result<DashboardDto>.Success(dashboard);
    }
}
