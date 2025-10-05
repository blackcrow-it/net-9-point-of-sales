using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

public record CreateInventoryIssueCommand(
    Guid StoreId,
    IssueType Type,
    Guid? DestinationStoreId,
    List<InventoryIssueItemDto> Items,
    string? Reason
) : IRequest<Result<Guid>>;

public enum IssueType
{
    Adjustment = 0,
    Damage = 1,
    Loss = 2,
    Transfer = 3,
    Return = 4
}

public record InventoryIssueItemDto(
    Guid ProductVariantId,
    decimal Quantity,
    string? Notes
);
