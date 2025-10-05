using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class CreateInventoryIssueCommandHandler : IRequestHandler<CreateInventoryIssueCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateInventoryIssueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateInventoryIssueCommand request, CancellationToken cancellationToken)
    {
        // Validate transfer requires destination
        if (request.Type == IssueType.Transfer && !request.DestinationStoreId.HasValue)
            return Result<Guid>.Failure("Destination store is required for transfers");

        // Generate issue number
        var today = DateTime.UtcNow.Date;
        var todayIssuesCount = await _context.InventoryIssues
            .CountAsync(i => i.IssueDate >= today && i.IssueDate < today.AddDays(1), cancellationToken);
        var issueNumber = $"ISS-{DateTime.UtcNow:yyyyMMdd}-{(todayIssuesCount + 1):D4}";

        // Map IssueType enum to domain IssueType
        var domainIssueType = request.Type switch
        {
            IssueType.Adjustment => Domain.Entities.Inventory.IssueType.Adjustment,
            IssueType.Damage => Domain.Entities.Inventory.IssueType.Damage,
            IssueType.Loss => Domain.Entities.Inventory.IssueType.Loss,
            IssueType.Transfer => Domain.Entities.Inventory.IssueType.Transfer,
            IssueType.Return => Domain.Entities.Inventory.IssueType.Return,
            _ => Domain.Entities.Inventory.IssueType.Adjustment
        };

        // Create inventory issue
        var issue = new InventoryIssue
        {
            Id = Guid.NewGuid(),
            IssueNumber = issueNumber,
            StoreId = request.StoreId,
            DestinationStoreId = request.DestinationStoreId,
            Type = domainIssueType,
            Status = IssueStatus.Draft,
            IssueDate = DateTime.UtcNow,
            Reason = request.Reason
        };

        await _context.InventoryIssues.AddAsync(issue, cancellationToken);

        // Create issue items
        int lineNumber = 1;
        foreach (var itemDto in request.Items)
        {
            var productVariant = await _context.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId, cancellationToken);

            if (productVariant == null)
                return Result<Guid>.Failure($"Product variant not found for item {lineNumber}");

            var issueItem = new InventoryIssueItem
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                ProductVariantId = itemDto.ProductVariantId,
                LineNumber = lineNumber++,
                Quantity = itemDto.Quantity,
                Notes = itemDto.Notes
            };

            await _context.InventoryIssueItems.AddAsync(issueItem, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(issue.Id);
    }
}
