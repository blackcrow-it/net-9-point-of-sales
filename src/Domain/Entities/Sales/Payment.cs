using Domain.Common;

namespace Domain.Entities.Sales;

/// <summary>
/// Represents a payment transaction for an order
/// </summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Payment number with format: PAY-YYYYMMDD-XXXX
    /// </summary>
    public string PaymentNumber { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }

    /// <summary>
    /// External transaction ID (required for electronic payments)
    /// </summary>
    public string? ReferenceNumber { get; set; }

    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Marks the payment as completed
    /// </summary>
    public void MarkAsCompleted(string? processedBy = null)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Payment is already completed");

        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        ProcessedBy = processedBy;
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    public void MarkAsFailed()
    {
        if (Status == PaymentStatus.Failed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot mark payment as failed from status {Status}");

        Status = PaymentStatus.Failed;
    }

    /// <summary>
    /// Refunds the payment
    /// </summary>
    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded");

        Status = PaymentStatus.Refunded;
    }

    /// <summary>
    /// Validates that Amount > 0
    /// </summary>
    public void Validate()
    {
        if (Amount <= 0)
            throw new ArgumentException("Amount must be > 0", nameof(Amount));
    }
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}
