using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class FinalizeStocktakeCommandHandler : IRequestHandler<FinalizeStocktakeCommand, Result<StocktakeSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public FinalizeStocktakeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StocktakeSummaryDto>> Handle(FinalizeStocktakeCommand request, CancellationToken cancellationToken)
    {
        // Retrieve stocktake with items
        var stocktake = await _context.Stocktakes
            .Include(s => s.Items)
                .ThenInclude(si => si.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
            .FirstOrDefaultAsync(s => s.Id == request.StocktakeId, cancellationToken);

        if (stocktake == null)
            return Result<StocktakeSummaryDto>.Failure("Stocktake not found");

        if (stocktake.Status == StocktakeStatus.Completed)
            return Result<StocktakeSummaryDto>.Failure("Stocktake already completed");

        if (stocktake.Status != StocktakeStatus.InProgress)
            return Result<StocktakeSummaryDto>.Failure("Stocktake must be in progress before finalizing");

        var varianceItems = new List<VarianceItemDto>();
        decimal totalVarianceValue = 0;
        int itemsWithVariance = 0;

        // Generate issue number for adjustment
        var today = DateTime.UtcNow;
        var dailyCount = await _context.InventoryIssues
            .CountAsync(i => i.CreatedAt.Date == today.Date, cancellationToken);
        var issueNumber = $"ISS-{today:yyyyMMdd}-{(dailyCount + 1):D4}";

        // Create inventory issue for adjustments
        InventoryIssue? adjustmentIssue = null;

        // Process each stocktake item
        foreach (var item in stocktake.Items)
        {
            var variance = item.CountedQuantity - item.SystemQuantity;

            if (variance != 0)
            {
                itemsWithVariance++;

                // Get inventory level
                var inventoryLevel = await _context.InventoryLevels
                    .FirstOrDefaultAsync(
                        il => il.StoreId == stocktake.StoreId && il.ProductVariantId == item.ProductVariantId,
                        cancellationToken
                    );

                if (inventoryLevel == null)
                    return Result<StocktakeSummaryDto>.Failure($"Inventory level not found for product variant {item.ProductVariantId}");

                // Get unit cost
                var unitCost = item.ProductVariant?.Product?.CostPrice ?? 0;
                var varianceValue = variance * unitCost;
                totalVarianceValue += varianceValue;

                // Create adjustment issue if not exists
                if (adjustmentIssue == null)
                {
                    adjustmentIssue = new InventoryIssue
                    {
                        Id = Guid.NewGuid(),
                        IssueNumber = issueNumber,
                        StoreId = stocktake.StoreId,
                        Type = Domain.Entities.Inventory.IssueType.Adjustment,
                        Status = Domain.Entities.Inventory.IssueStatus.Completed,
                        Reason = $"Stocktake adjustment - {stocktake.StocktakeNumber}",
                        IssueDate = DateTime.UtcNow,
                        Items = new List<InventoryIssueItem>()
                    };
                    _context.InventoryIssues.Add(adjustmentIssue);
                }

                // Add adjustment item (use absolute value, sign handled in inventory update)
                adjustmentIssue.Items.Add(new InventoryIssueItem
                {
                    Id = Guid.NewGuid(),
                    IssueId = adjustmentIssue.Id,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = Math.Abs(variance),
                    LineNumber = adjustmentIssue.Items.Count + 1,
                    Unit = item.Unit,
                    Notes = variance > 0 ? "Stock surplus" : "Stock shortage"
                });

                // Update inventory level to match counted quantity
                inventoryLevel.OnHandQuantity = item.CountedQuantity;
                inventoryLevel.AvailableQuantity = item.CountedQuantity - inventoryLevel.ReservedQuantity;

                // Add to variance report
                varianceItems.Add(new VarianceItemDto(
                    item.ProductVariantId,
                    item.ProductVariant?.Product?.Name ?? "Unknown",
                    item.ProductVariant?.Name ?? "Unknown",
                    (int)item.SystemQuantity,
                    (int)item.CountedQuantity,
                    (int)variance,
                    unitCost,
                    varianceValue
                ));
            }
        }

        // Update stocktake status - use Complete() method if available
        stocktake.Status = StocktakeStatus.Completed;

        await _context.SaveChangesAsync(cancellationToken);

        var summary = new StocktakeSummaryDto(
            stocktake.Id,
            stocktake.StocktakeNumber,
            stocktake.Items.Count,
            itemsWithVariance,
            totalVarianceValue,
            varianceItems
        );

        return Result<StocktakeSummaryDto>.Success(summary);
    }
}
