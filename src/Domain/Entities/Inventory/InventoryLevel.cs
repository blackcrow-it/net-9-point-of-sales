using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Current stock levels per store/warehouse
/// </summary>
public class InventoryLevel : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public Guid StoreId { get; set; }

    public decimal AvailableQuantity { get; set; }

    /// <summary>
    /// Quantity on hold for orders
    /// </summary>
    public decimal ReservedQuantity { get; set; }

    /// <summary>
    /// Physical count: AvailableQuantity + ReservedQuantity
    /// </summary>
    public decimal OnHandQuantity { get; set; }

    public DateTime? LastCountedAt { get; set; }
    public string? LastCountedBy { get; set; }

    // Navigation Properties
    public ProductVariant ProductVariant { get; set; } = null!;
    public Store.Store Store { get; set; } = null!;

    /// <summary>
    /// Reserves stock for an order
    /// </summary>
    public void ReserveStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be > 0", nameof(quantity));

        if (AvailableQuantity < quantity)
            throw new InvalidOperationException("Insufficient available quantity");

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
    }

    /// <summary>
    /// Releases reserved stock
    /// </summary>
    public void ReleaseStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be > 0", nameof(quantity));

        if (ReservedQuantity < quantity)
            throw new InvalidOperationException("Insufficient reserved quantity");

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
    }

    /// <summary>
    /// Adjusts available quantity
    /// </summary>
    public void AdjustQuantity(decimal delta)
    {
        AvailableQuantity += delta;
        OnHandQuantity = AvailableQuantity + ReservedQuantity;

        if (AvailableQuantity < 0)
            throw new InvalidOperationException("Available quantity cannot be negative");
    }

    /// <summary>
    /// Validates business rule: OnHandQuantity = AvailableQuantity + ReservedQuantity
    /// </summary>
    public void Validate()
    {
        var expectedOnHand = AvailableQuantity + ReservedQuantity;
        if (Math.Abs(OnHandQuantity - expectedOnHand) > 0.001m)
            throw new InvalidOperationException("OnHandQuantity must equal AvailableQuantity + ReservedQuantity");
    }
}
