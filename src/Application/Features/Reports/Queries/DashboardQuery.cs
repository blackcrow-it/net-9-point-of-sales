using Application.Common.Models;
using MediatR;

namespace Application.Features.Reports.Queries;

public record DashboardQuery(
    Guid StoreId
) : IRequest<Result<DashboardDto>>;

public record DashboardDto(
    decimal TodaySales,
    int TodayOrders,
    int ActiveCustomers,
    int LowStockCount,
    decimal WeekSales,
    int WeekOrders,
    decimal MonthSales,
    int MonthOrders,
    List<TopSellingProductDto> TopSellingProducts,
    DateTime GeneratedAt
);

public record TopSellingProductDto(
    Guid ProductId,
    string ProductName,
    decimal TotalRevenue,
    decimal TotalQuantity
);
