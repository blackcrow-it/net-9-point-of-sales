using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Physical inventory counts
/// </summary>
public class Stocktake : BaseEntity
{
    /// <summary>
    /// Stocktake number with format: STK-YYYYMMDD-XXXX
    /// </summary>
    public string StocktakeNumber { get; set; } = string.Empty;

    public Guid StoreId { get; set; }

    public StocktakeStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public string? Notes { get; set; }
    public string? CountedBy { get; set; }

    // Navigation Properties
    public Store.Store Store { get; set; } = null!;
    public ICollection<StocktakeItem> Items { get; set; } = new List<StocktakeItem>();

    /// <summary>
    /// Starts the stocktake
    /// </summary>
    public void Start()
    {
        if (Status != StocktakeStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start stocktake with status {Status}");

        Status = StocktakeStatus.InProgress;
    }

    /// <summary>
    /// Completes the stocktake
    /// </summary>
    public void Complete()
    {
        if (Status != StocktakeStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete stocktake with status {Status}");

        Status = StocktakeStatus.Completed;
        CompletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the stocktake
    /// </summary>
    public void Cancel()
    {
        if (Status == StocktakeStatus.Cancelled)
            throw new InvalidOperationException("Stocktake is already cancelled");

        if (Status == StocktakeStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed stocktake");

        Status = StocktakeStatus.Cancelled;
    }

    /// <summary>
    /// Validates stocktake constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(StocktakeNumber))
            throw new ArgumentException("Stocktake number is required", nameof(StocktakeNumber));

        if (Status == StocktakeStatus.Completed && !CompletedDate.HasValue)
            throw new InvalidOperationException("Completed date is required when status is Completed");

        if (CompletedDate.HasValue && CompletedDate.Value < ScheduledDate)
            throw new ArgumentException("Completed date must be >= scheduled date", nameof(CompletedDate));
    }
}

public enum StocktakeStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
