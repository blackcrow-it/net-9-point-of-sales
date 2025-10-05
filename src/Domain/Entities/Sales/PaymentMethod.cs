using Domain.Common;

namespace Domain.Entities.Sales;

/// <summary>
/// Configuration for available payment methods
/// </summary>
public class PaymentMethod : BaseEntity
{
    /// <summary>
    /// Display name (e.g., "Cash", "Card", "VNPay", "MoMo")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code (e.g., "CASH", "CARD", "VNPAY", "MOMO")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public PaymentMethodType Type { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Whether external transaction ID is required
    /// </summary>
    public bool RequiresReference { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// JSON configuration for gateway settings
    /// </summary>
    public string? Configuration { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// Validates payment method constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new ArgumentException("Code is required", nameof(Code));

        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name is required", nameof(Name));

        if (DisplayOrder < 0)
            throw new ArgumentException("Display order must be >= 0", nameof(DisplayOrder));
    }
}

public enum PaymentMethodType
{
    Cash = 0,
    Card = 1,
    EWallet = 2,
    BankTransfer = 3,
    Other = 99
}
