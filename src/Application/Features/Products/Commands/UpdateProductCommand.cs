using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string SKU,
    string? Barcode,
    Guid? CategoryId,
    Guid? BrandId,
    decimal Price,
    decimal? Cost,
    string? Description,
    bool TrackInventory,
    bool IsActive
) : IRequest<Result>;
