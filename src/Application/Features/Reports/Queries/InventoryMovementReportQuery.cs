using Application.Common.Models;
using MediatR;

namespace Application.Features.Reports.Queries;

public record InventoryMovementReportQuery(
    Guid StoreId,
    Guid? ProductId,
    DateTime DateFrom,
    DateTime DateTo
) : IRequest<Result<InventoryMovementReportDto>>;

public record InventoryMovementReportDto(
    Guid StoreId,
    Guid? ProductId,
    DateTime DateFrom,
    DateTime DateTo,
    decimal OpeningStock,
    decimal Receipts,
    decimal Issues,
    decimal Sales,
    decimal ClosingStock,
    List<InventoryMovementDetail> Details
);

public record InventoryMovementDetail(
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    decimal OpeningStock,
    decimal Receipts,
    decimal Issues,
    decimal Sales,
    decimal ClosingStock
);
