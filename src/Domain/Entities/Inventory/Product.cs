using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Master product data
/// </summary>
public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    public ProductType Type { get; set; }

    /// <summary>
    /// Base unit (e.g., "pcs", "kg", etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    public decimal? CostPrice { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }

    public bool TrackInventory { get; set; }
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQuantity { get; set; }

    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Comma-separated tags
    /// </summary>
    public string? Tags { get; set; }

    // Navigation Properties
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    /// <summary>
    /// Validates product constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SKU))
            throw new ArgumentException("SKU is required", nameof(SKU));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));

        if (CostPrice.HasValue && CostPrice.Value < 0)
            throw new ArgumentException("Cost price must be >= 0", nameof(CostPrice));

        if (RetailPrice.HasValue && RetailPrice.Value < 0)
            throw new ArgumentException("Retail price must be >= 0", nameof(RetailPrice));

        if (TrackInventory && (!ReorderLevel.HasValue || !ReorderQuantity.HasValue))
            throw new InvalidOperationException("Reorder level and quantity are required when tracking inventory");
    }
}

public enum ProductType
{
    /// <summary>
    /// No variants
    /// </summary>
    Single = 0,

    /// <summary>
    /// Has variants (e.g., size, color)
    /// </summary>
    Variable = 1
}
