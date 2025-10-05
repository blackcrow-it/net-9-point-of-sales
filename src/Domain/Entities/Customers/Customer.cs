using Domain.Common;

namespace Domain.Entities.Customers;

/// <summary>
/// Customer master data
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Customer number with format: CUS-XXXXXXXX
    /// </summary>
    public string CustomerNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public Guid? CustomerGroupId { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public CustomerGender? Gender { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }

    public decimal LoyaltyPoints { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }

    public DateTime? LastPurchaseDate { get; set; }

    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public CustomerGroup? CustomerGroup { get; set; }
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    public ICollection<Debt> Debts { get; set; } = new List<Debt>();

    /// <summary>
    /// Adds loyalty points to the customer
    /// </summary>
    public void AddLoyaltyPoints(decimal points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be > 0", nameof(points));

        LoyaltyPoints += points;
    }

    /// <summary>
    /// Redeems loyalty points
    /// </summary>
    public void RedeemPoints(decimal points)
    {
        if (points <= 0)
            throw new ArgumentException("Points must be > 0", nameof(points));

        if (LoyaltyPoints < points)
            throw new InvalidOperationException("Insufficient loyalty points");

        LoyaltyPoints -= points;
    }

    /// <summary>
    /// Records a purchase for the customer
    /// </summary>
    public void RecordPurchase(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be > 0", nameof(amount));

        TotalSpent += amount;
        TotalOrders++;
        LastPurchaseDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates customer constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CustomerNumber))
            throw new ArgumentException("Customer number is required", nameof(CustomerNumber));

        if (string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Email))
            throw new InvalidOperationException("Either phone or email is required");

        // Vietnamese phone validation: 10 digits
        if (!string.IsNullOrWhiteSpace(Phone) && !System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^\d{10}$"))
            throw new ArgumentException("Phone must be 10 digits (Vietnamese format)", nameof(Phone));

        // Email validation
        if (!string.IsNullOrWhiteSpace(Email) && !System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Email must be valid format", nameof(Email));
    }
}

public enum CustomerGender
{
    Male = 0,
    Female = 1,
    Other = 2
}
