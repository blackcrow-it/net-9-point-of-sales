using Domain.Common;

namespace Domain.Entities.Sales;

/// <summary>
/// Represents a line item in an order
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Sequential line number within the order
    /// </summary>
    public int LineNumber { get; set; }

    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public Inventory.ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>
    /// Calculates line total: (Quantity * UnitPrice - DiscountAmount) + TaxAmount
    /// </summary>
    public void CalculateLineTotal()
    {
        var subtotal = Quantity * UnitPrice - DiscountAmount;
        TaxAmount = subtotal * (TaxRate / 100);
        LineTotal = subtotal + TaxAmount;
    }

    /// <summary>
    /// Validates that Quantity > 0 and UnitPrice >= 0
    /// </summary>
    public void Validate()
    {
        if (Quantity <= 0)
            throw new ArgumentException("Quantity must be > 0", nameof(Quantity));

        if (UnitPrice < 0)
            throw new ArgumentException("Unit price must be >= 0", nameof(UnitPrice));
    }
}
