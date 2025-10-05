using Domain.Common;

namespace Domain.Entities.Sales;

/// <summary>
/// Represents a sales transaction at POS
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Order number with format: ORD-YYYYMMDD-XXXX
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// User who created the order (cashier)
    /// </summary>
    public Guid CashierId { get; set; }
    public Guid ShiftId { get; set; }

    public OrderStatus Status { get; set; }
    public OrderType Type { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }

    // Navigation Properties
    public Store.Store Store { get; set; } = null!;
    public Customers.Customer? Customer { get; set; }
    public Employees.User Cashier { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Validates that OrderNumber is required and Subtotal is >= 0
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OrderNumber))
            throw new ArgumentException("Order number is required", nameof(OrderNumber));

        if (Subtotal < 0)
            throw new ArgumentException("Subtotal must be >= 0", nameof(Subtotal));
    }

    /// <summary>
    /// Calculates total amount: Subtotal + TaxAmount - DiscountAmount
    /// </summary>
    public void CalculateTotalAmount()
    {
        TotalAmount = Subtotal + TaxAmount - DiscountAmount;
    }

    /// <summary>
    /// Marks the order as completed
    /// </summary>
    public void Complete()
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.OnHold)
            throw new InvalidOperationException($"Cannot complete order with status {Status}");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Voids the order with a reason
    /// </summary>
    public void Void(string reason)
    {
        if (Status == OrderStatus.Voided)
            throw new InvalidOperationException("Order is already voided");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Void reason is required", nameof(reason));

        Status = OrderStatus.Voided;
        VoidedAt = DateTime.UtcNow;
        VoidReason = reason;
    }

    /// <summary>
    /// Puts the order on hold
    /// </summary>
    public void Hold()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Cannot hold order with status {Status}");

        Status = OrderStatus.OnHold;
    }

    /// <summary>
    /// Adds a payment to the order
    /// </summary>
    public void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        Payments.Add(payment);
    }
}

public enum OrderStatus
{
    Draft = 0,
    Completed = 1,
    Voided = 2,
    Returned = 3,
    OnHold = 4
}

public enum OrderType
{
    Sale = 0,
    Return = 1,
    Exchange = 2
}
