using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

/// <summary>
/// Command to generate unique barcodes for product variants
/// </summary>
public record GenerateBarcodesCommand(
    List<Guid> ProductVariantIds
) : IRequest<Result<List<BarcodeDto>>>;

public record BarcodeDto(
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    string Barcode,
    string SKU
);
