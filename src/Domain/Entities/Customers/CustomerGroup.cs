using Domain.Common;

namespace Domain.Entities.Customers;

/// <summary>
/// Customer segmentation for pricing/promotions
/// </summary>
public class CustomerGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Loyalty points multiplier (default 1.0)
    /// </summary>
    public decimal LoyaltyPointsMultiplier { get; set; } = 1.0m;

    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();

    /// <summary>
    /// Validates customer group constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));

        if (DiscountPercentage < 0 || DiscountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(DiscountPercentage));

        if (LoyaltyPointsMultiplier < 0)
            throw new ArgumentException("Loyalty points multiplier must be >= 0", nameof(LoyaltyPointsMultiplier));
    }
}
