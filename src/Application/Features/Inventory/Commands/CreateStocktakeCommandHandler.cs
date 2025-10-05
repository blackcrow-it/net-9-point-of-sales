using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class CreateStocktakeCommandHandler : IRequestHandler<CreateStocktakeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateStocktakeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateStocktakeCommand request, CancellationToken cancellationToken)
    {
        // Generate stocktake number
        var today = DateTime.UtcNow.Date;
        var todayStocktakesCount = await _context.Stocktakes
            .CountAsync(s => s.CreatedAt >= today && s.CreatedAt < today.AddDays(1), cancellationToken);
        var stocktakeNumber = $"STK-{DateTime.UtcNow:yyyyMMdd}-{(todayStocktakesCount + 1):D4}";

        // Create stocktake
        var stocktake = new Stocktake
        {
            Id = Guid.NewGuid(),
            StocktakeNumber = stocktakeNumber,
            StoreId = request.StoreId,
            Status = StocktakeStatus.Scheduled,
            ScheduledDate = request.ScheduledDate,
            Notes = request.Notes
        };

        await _context.Stocktakes.AddAsync(stocktake, cancellationToken);

        // Create stocktake items for all variants of specified products
        var productVariants = await _context.ProductVariants
            .Include(pv => pv.Product)
            .Where(pv => request.ProductIds.Contains(pv.ProductId) && pv.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var variant in productVariants)
        {
            // Get current inventory level
            var inventoryLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(il => il.ProductVariantId == variant.Id && il.StoreId == request.StoreId, cancellationToken);

            var stocktakeItem = new StocktakeItem
            {
                Id = Guid.NewGuid(),
                StocktakeId = stocktake.Id,
                ProductVariantId = variant.Id,
                SystemQuantity = inventoryLevel?.OnHandQuantity ?? 0,
                CountedQuantity = 0, // Will be updated during counting
                Variance = 0,
                Unit = variant.Product.Unit ?? "pcs"
            };

            await _context.StocktakeItems.AddAsync(stocktakeItem, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(stocktake.Id);
    }
}
