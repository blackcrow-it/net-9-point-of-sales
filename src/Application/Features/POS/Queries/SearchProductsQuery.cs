using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Queries;

public record SearchProductsQuery(
    string? SearchTerm,
    Guid StoreId,
    Guid? CategoryId = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<ProductSearchResultDto>>>;

public record ProductSearchResultDto(
    Guid Id,
    Guid? ProductId,
    string SKU,
    string Name,
    string? VariantName,
    string? Barcode,
    decimal RetailPrice,
    decimal AvailableQuantity,
    string Unit,
    bool TrackInventory,
    string? CategoryName,
    string? ImageUrl
);
