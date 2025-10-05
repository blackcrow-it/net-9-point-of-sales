using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

public record CreateInventoryReceiptCommand(
    Guid StoreId,
    Guid SupplierId,
    string ReceiptNumber,
    DateTime ReceiptDate,
    List<ReceiptItemDto> Items,
    string? Notes = null
) : IRequest<Result<Guid>>;

public record ReceiptItemDto(
    Guid ProductId,
    Guid? ProductVariantId,
    decimal Quantity,
    decimal UnitCost
);
