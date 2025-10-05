using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Line items in goods receipt
/// </summary>
public class InventoryReceiptItem : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int LineNumber { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public InventoryReceipt Receipt { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>
    /// Calculates line total: Quantity * UnitCost
    /// </summary>
    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitCost;
    }

    /// <summary>
    /// Validates constraints
    /// </summary>
    public void Validate()
    {
        if (Quantity <= 0)
            throw new ArgumentException("Quantity must be > 0", nameof(Quantity));

        if (UnitCost < 0)
            throw new ArgumentException("Unit cost must be >= 0", nameof(UnitCost));
    }
}
