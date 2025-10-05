using Domain.Common;

namespace Domain.Entities.Customers;

/// <summary>
/// Loyalty points earning/redemption history
/// </summary>
public class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    public LoyaltyTransactionType Type { get; set; }
    public decimal Points { get; set; }

    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public Sales.Order? Order { get; set; }

    /// <summary>
    /// Validates loyalty transaction constraints
    /// </summary>
    public void Validate()
    {
        if (Type == LoyaltyTransactionType.Earned && Points <= 0)
            throw new ArgumentException("Points must be > 0 for Earned transactions", nameof(Points));

        if ((Type == LoyaltyTransactionType.Redeemed || Type == LoyaltyTransactionType.Expired) && Points >= 0)
            throw new ArgumentException("Points must be < 0 for Redeemed/Expired transactions", nameof(Points));

        if ((Type == LoyaltyTransactionType.Earned || Type == LoyaltyTransactionType.Redeemed) && !OrderId.HasValue)
            throw new InvalidOperationException("Order ID is required for Earned/Redeemed transactions");
    }
}

public enum LoyaltyTransactionType
{
    Earned = 0,
    Redeemed = 1,
    Adjusted = 2,
    Expired = 3
}
