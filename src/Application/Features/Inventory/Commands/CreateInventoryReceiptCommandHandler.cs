using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class CreateInventoryReceiptCommandHandler : IRequestHandler<CreateInventoryReceiptCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateInventoryReceiptCommand request, CancellationToken cancellationToken)
    {
        // Generate receipt number
        var today = DateTime.UtcNow.Date;
        var todayReceiptsCount = await _context.InventoryReceipts
            .CountAsync(r => r.ReceiptDate >= today && r.ReceiptDate < today.AddDays(1), cancellationToken);
        var receiptNumber = $"GRN-{DateTime.UtcNow:yyyyMMdd}-{(todayReceiptsCount + 1):D4}";

        // Create inventory receipt
        var receipt = new InventoryReceipt
        {
            Id = Guid.NewGuid(),
            ReceiptNumber = receiptNumber,
            StoreId = request.StoreId,
            SupplierId = request.SupplierId,
            ReceiptDate = request.ReceiptDate,
            Status = ReceiptStatus.Draft,
            TotalAmount = 0,
            Notes = request.Notes
        };

        // Create receipt items
        int lineNumber = 1;
        foreach (var itemDto in request.Items)
        {
            var variantId = itemDto.ProductVariantId ?? itemDto.ProductId;
            var productVariant = await _context.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == variantId, cancellationToken);

            if (productVariant == null)
                return Result<Guid>.Failure($"Product variant not found for item {lineNumber}");

            var receiptItem = new InventoryReceiptItem
            {
                Id = Guid.NewGuid(),
                ReceiptId = receipt.Id,
                ProductVariantId = variantId,
                LineNumber = lineNumber++,
                Quantity = itemDto.Quantity,
                UnitCost = itemDto.UnitCost,
                LineTotal = itemDto.Quantity * itemDto.UnitCost,
                Notes = null
            };

            receipt.TotalAmount += receiptItem.LineTotal;
            await _context.InventoryReceiptItems.AddAsync(receiptItem, cancellationToken);
        }

        await _context.InventoryReceipts.AddAsync(receipt, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(receipt.Id);
    }
}
