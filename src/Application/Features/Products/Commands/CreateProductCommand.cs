using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string SKU,
    string? Barcode,
    Guid? CategoryId,
    Guid? BrandId,
    decimal Price,
    decimal? Cost,
    string? Description,
    Guid StoreId,
    bool TrackInventory = true,
    bool IsActive = true
) : IRequest<Result<Guid>>;
