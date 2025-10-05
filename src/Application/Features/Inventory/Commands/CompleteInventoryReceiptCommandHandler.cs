using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class CompleteInventoryReceiptCommandHandler : IRequestHandler<CompleteInventoryReceiptCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CompleteInventoryReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CompleteInventoryReceiptCommand request, CancellationToken cancellationToken)
    {
        // Find receipt with items
        var receipt = await _context.InventoryReceipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.ReceiptId, cancellationToken);

        if (receipt == null)
            return Result<Guid>.Failure("Inventory receipt not found");

        // Complete receipt using domain method
        try
        {
            receipt.Complete();
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        // Update inventory levels for each item
        foreach (var item in receipt.Items)
        {
            var inventoryLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(il => il.ProductVariantId == item.ProductVariantId && il.StoreId == receipt.StoreId, cancellationToken);

            if (inventoryLevel == null)
            {
                // Create new inventory level if doesn't exist
                inventoryLevel = new InventoryLevel
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = item.ProductVariantId,
                    StoreId = receipt.StoreId,
                    OnHandQuantity = 0,
                    AvailableQuantity = 0,
                    ReservedQuantity = 0
                };
                await _context.InventoryLevels.AddAsync(inventoryLevel, cancellationToken);
            }

            // Increment stock
            inventoryLevel.OnHandQuantity += item.Quantity;
            inventoryLevel.AvailableQuantity += item.Quantity;
            inventoryLevel.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(receipt.Id);
    }
}
