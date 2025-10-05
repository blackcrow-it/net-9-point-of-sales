using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class CompleteInventoryIssueCommandHandler : IRequestHandler<CompleteInventoryIssueCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CompleteInventoryIssueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CompleteInventoryIssueCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the inventory issue
        var issue = await _context.InventoryIssues
            .Include(i => i.Items)
            .ThenInclude(ii => ii.ProductVariant)
            .FirstOrDefaultAsync(i => i.Id == request.IssueId, cancellationToken);

        if (issue == null)
            return Result<Guid>.Failure("Inventory issue not found");

        if (issue.Status == IssueStatus.Completed)
            return Result<Guid>.Failure("Inventory issue already completed");

        // Update issue status using domain method
        issue.Complete();

        // Process each item and update inventory levels
        foreach (var item in issue.Items)
        {
            // Get source inventory level
            var sourceLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(
                    il => il.StoreId == issue.StoreId && il.ProductVariantId == item.ProductVariantId,
                    cancellationToken
                );

            if (sourceLevel == null)
                return Result<Guid>.Failure($"Inventory level not found for product variant {item.ProductVariantId} at store {issue.StoreId}");

            // Validate sufficient stock
            if (sourceLevel.AvailableQuantity < item.Quantity)
                return Result<Guid>.Failure($"Insufficient stock for {item.ProductVariant?.Name}. Available: {sourceLevel.AvailableQuantity}, Required: {item.Quantity}");

            // Decrement source inventory
            sourceLevel.OnHandQuantity -= item.Quantity;
            sourceLevel.AvailableQuantity -= item.Quantity;

            // If transfer, increment destination inventory
            if (issue.Type == Domain.Entities.Inventory.IssueType.Transfer && issue.DestinationStoreId.HasValue)
            {
                var destLevel = await _context.InventoryLevels
                    .FirstOrDefaultAsync(
                        il => il.StoreId == issue.DestinationStoreId.Value && il.ProductVariantId == item.ProductVariantId,
                        cancellationToken
                    );

                if (destLevel == null)
                {
                    // Create new inventory level at destination
                    destLevel = new InventoryLevel
                    {
                        Id = Guid.NewGuid(),
                        StoreId = issue.DestinationStoreId.Value,
                        ProductVariantId = item.ProductVariantId,
                        OnHandQuantity = item.Quantity,
                        AvailableQuantity = item.Quantity,
                        ReservedQuantity = 0
                    };
                    _context.InventoryLevels.Add(destLevel);
                }
                else
                {
                    destLevel.OnHandQuantity += item.Quantity;
                    destLevel.AvailableQuantity += item.Quantity;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(issue.Id);
    }
}
