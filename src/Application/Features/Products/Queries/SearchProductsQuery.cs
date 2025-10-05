using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries;

public record SearchProductsQuery(
    Guid StoreId,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool? IsActive = true,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<ProductDto>>>;

public record ProductDto(
    Guid Id,
    string Name,
    string SKU,
    string? Barcode,
    Guid? CategoryId,
    string? CategoryName,
    Guid? BrandId,
    string? BrandName,
    decimal Price,
    decimal? Cost,
    bool TrackInventory,
    decimal? AvailableQuantity,
    bool IsActive
);
