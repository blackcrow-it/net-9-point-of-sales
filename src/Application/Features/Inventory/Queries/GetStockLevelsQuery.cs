using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Queries;

/// <summary>
/// Query to retrieve current stock levels with optional filters
/// </summary>
public record GetStockLevelsQuery(
    Guid StoreId,
    Guid? ProductId = null,
    bool? LowStock = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PaginatedList<StockLevelDto>>>;

public record StockLevelDto(
    Guid Id,
    Guid StoreId,
    string StoreName,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid ProductVariantId,
    string VariantName,
    decimal OnHandQuantity,
    decimal AvailableQuantity,
    decimal ReservedQuantity,
    DateTime? LastCountedAt
);
