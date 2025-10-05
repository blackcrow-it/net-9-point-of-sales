using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Inventory adjustments (damage, theft, transfer)
/// </summary>
public class InventoryIssue : BaseEntity
{
    /// <summary>
    /// Issue number with format: ISS-YYYYMMDD-XXXX
    /// </summary>
    public string IssueNumber { get; set; } = string.Empty;

    public Guid StoreId { get; set; }

    public IssueType Type { get; set; }
    public IssueStatus Status { get; set; }

    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Destination store for transfers
    /// </summary>
    public Guid? DestinationStoreId { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation Properties
    public Store.Store Store { get; set; } = null!;
    public Store.Store? DestinationStore { get; set; }
    public ICollection<InventoryIssueItem> Items { get; set; } = new List<InventoryIssueItem>();

    /// <summary>
    /// Completes the issue
    /// </summary>
    public void Complete()
    {
        if (Status != IssueStatus.Draft)
            throw new InvalidOperationException($"Cannot complete issue with status {Status}");

        Status = IssueStatus.Completed;
    }

    /// <summary>
    /// Cancels the issue
    /// </summary>
    public void Cancel()
    {
        if (Status == IssueStatus.Cancelled)
            throw new InvalidOperationException("Issue is already cancelled");

        if (Status == IssueStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed issue");

        Status = IssueStatus.Cancelled;
    }

    /// <summary>
    /// Validates issue constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(IssueNumber))
            throw new ArgumentException("Issue number is required", nameof(IssueNumber));

        if (string.IsNullOrWhiteSpace(Reason))
            throw new ArgumentException("Reason is required", nameof(Reason));

        if (Type == IssueType.Transfer && !DestinationStoreId.HasValue)
            throw new InvalidOperationException("Destination store is required for transfers");

        if (Type == IssueType.Transfer && DestinationStoreId.HasValue && DestinationStoreId.Value == StoreId)
            throw new InvalidOperationException("Destination store must differ from source store");
    }
}

public enum IssueType
{
    Adjustment = 0,
    Damage = 1,
    Loss = 2,
    Transfer = 3,
    Return = 4
}

public enum IssueStatus
{
    Draft = 0,
    Completed = 1,
    Cancelled = 2
}
