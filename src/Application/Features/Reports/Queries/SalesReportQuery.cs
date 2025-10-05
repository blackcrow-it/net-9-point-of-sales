using Application.Common.Models;
using MediatR;

namespace Application.Features.Reports.Queries;

public record SalesReportQuery(
    Guid StoreId,
    DateTime DateFrom,
    DateTime DateTo,
    string GroupBy = "Day" // Day, Week, Month
) : IRequest<Result<SalesReportDto>>;

public record SalesReportDto(
    Guid StoreId,
    DateTime DateFrom,
    DateTime DateTo,
    string GroupBy,
    decimal TotalSales,
    int TotalOrders,
    decimal AverageOrderValue,
    decimal TotalTax,
    decimal TotalDiscount,
    List<SalesDataPoint> TimeSeries
);

public record SalesDataPoint(
    DateTime Date,
    decimal Sales,
    int Orders,
    decimal AverageOrderValue
);
