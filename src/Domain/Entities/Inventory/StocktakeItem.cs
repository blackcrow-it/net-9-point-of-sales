using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Counted items in stocktake
/// </summary>
public class StocktakeItem : BaseEntity
{
    public Guid StocktakeId { get; set; }
    public Guid ProductVariantId { get; set; }

    public decimal SystemQuantity { get; set; }
    public decimal CountedQuantity { get; set; }

    /// <summary>
    /// Variance: CountedQuantity - SystemQuantity
    /// </summary>
    public decimal Variance { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // Navigation Properties
    public Stocktake Stocktake { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>
    /// Calculates variance: CountedQuantity - SystemQuantity
    /// </summary>
    public void CalculateVariance()
    {
        Variance = CountedQuantity - SystemQuantity;
    }
}
