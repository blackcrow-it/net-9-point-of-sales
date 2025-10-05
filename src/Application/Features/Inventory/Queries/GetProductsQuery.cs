using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Queries;

public record GetProductsQuery(
    Guid? CategoryId,
    Guid? BrandId,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<ProductDto>>>;

public record ProductDto(
    Guid Id,
    string SKU,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    Guid? BrandId,
    string? BrandName,
    decimal CostPrice,
    decimal RetailPrice,
    string Unit,
    bool TrackInventory,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    string? ImageUrl,
    bool IsActive,
    int VariantCount,
    DateTime CreatedAt
);
