using Application.Common.Models;
using MediatR;

namespace Application.Features.Reports.Queries;

public record TopProductsReportQuery(
    Guid StoreId,
    DateTime DateFrom,
    DateTime DateTo,
    int TopN = 20
) : IRequest<Result<List<TopProductDto>>>;

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int TotalQuantity,
    decimal TotalRevenue,
    int OrderCount
);
