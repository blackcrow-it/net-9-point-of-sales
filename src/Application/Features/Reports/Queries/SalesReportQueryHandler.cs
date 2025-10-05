using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries;

public class SalesReportQueryHandler : IRequestHandler<SalesReportQuery, Result<SalesReportDto>>
{
    private readonly IApplicationDbContext _context;

    public SalesReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SalesReportDto>> Handle(SalesReportQuery request, CancellationToken cancellationToken)
    {
        // Verify store exists
        var storeExists = await _context.Stores
            .AnyAsync(s => s.Id == request.StoreId, cancellationToken);

        if (!storeExists)
        {
            return Result<SalesReportDto>.Failure("Store not found");
        }

        // Get orders within date range
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == request.StoreId
                && o.CreatedAt >= request.DateFrom
                && o.CreatedAt <= request.DateTo
                && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        // Calculate totals
        var totalSales = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var totalTax = orders.Sum(o => o.TaxAmount);
        var totalDiscount = orders.Sum(o => o.DiscountAmount);

        // Group data by period
        var timeSeries = request.GroupBy.ToLower() switch
        {
            "day" => GroupByDay(orders),
            "week" => GroupByWeek(orders),
            "month" => GroupByMonth(orders),
            _ => GroupByDay(orders)
        };

        var report = new SalesReportDto(
            request.StoreId,
            request.DateFrom,
            request.DateTo,
            request.GroupBy,
            totalSales,
            totalOrders,
            averageOrderValue,
            totalTax,
            totalDiscount,
            timeSeries
        );

        return Result<SalesReportDto>.Success(report);
    }

    private List<SalesDataPoint> GroupByDay(List<Order> orders)
    {
        return orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesDataPoint(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Count(),
                g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            ))
            .OrderBy(d => d.Date)
            .ToList();
    }

    private List<SalesDataPoint> GroupByWeek(List<Order> orders)
    {
        return orders
            .GroupBy(o => GetWeekStart(o.CreatedAt))
            .Select(g => new SalesDataPoint(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Count(),
                g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            ))
            .OrderBy(d => d.Date)
            .ToList();
    }

    private List<SalesDataPoint> GroupByMonth(List<Order> orders)
    {
        return orders
            .GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1))
            .Select(g => new SalesDataPoint(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Count(),
                g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            ))
            .OrderBy(d => d.Date)
            .ToList();
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
