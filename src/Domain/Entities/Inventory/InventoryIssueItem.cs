using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Line items in inventory issues
/// </summary>
public class InventoryIssueItem : BaseEntity
{
    public Guid IssueId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int LineNumber { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    public decimal? UnitCost { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public InventoryIssue Issue { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>
    /// Validates constraints
    /// </summary>
    public void Validate()
    {
        if (Quantity <= 0)
            throw new ArgumentException("Quantity must be > 0", nameof(Quantity));
    }
}
