using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Product variations (size, color, etc.)
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Variant name (e.g., "Large - Red")
    /// </summary>
    public string? Name { get; set; }

    public string? Barcode { get; set; }

    public decimal? CostPrice { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Default variant for the product
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// JSON attributes (e.g., {"size": "L", "color": "Red"})
    /// </summary>
    public string? Attributes { get; set; }

    public string? ImageUrl { get; set; }

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public ICollection<InventoryLevel> InventoryLevels { get; set; } = new List<InventoryLevel>();

    /// <summary>
    /// Validates variant constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SKU))
            throw new ArgumentException("SKU is required", nameof(SKU));
    }
}
