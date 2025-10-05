using Domain.Common;

namespace Domain.Entities.Customers;

/// <summary>
/// Customer account receivables
/// </summary>
public class Debt : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Debt number with format: DBT-YYYYMMDD-XXXX
    /// </summary>
    public string DebtNumber { get; set; } = string.Empty;

    public DebtStatus Status { get; set; }

    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Remaining amount: Amount - PaidAmount
    /// </summary>
    public decimal RemainingAmount { get; set; }

    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    public Sales.Order? Order { get; set; }

    /// <summary>
    /// Records a payment towards the debt
    /// </summary>
    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be > 0", nameof(amount));

        if (amount > RemainingAmount)
            throw new InvalidOperationException("Payment amount exceeds remaining debt");

        PaidAmount += amount;
        CalculateRemainingAmount();

        if (RemainingAmount == 0)
        {
            Status = DebtStatus.Paid;
            PaidDate = DateTime.UtcNow;
        }
        else if (PaidAmount > 0)
        {
            Status = DebtStatus.PartiallyPaid;
        }
    }

    /// <summary>
    /// Marks the debt as overdue
    /// </summary>
    public void MarkOverdue()
    {
        if (Status == DebtStatus.Paid || Status == DebtStatus.WrittenOff)
            throw new InvalidOperationException($"Cannot mark debt as overdue from status {Status}");

        Status = DebtStatus.Overdue;
    }

    /// <summary>
    /// Writes off the debt
    /// </summary>
    public void WriteOff()
    {
        if (Status == DebtStatus.Paid)
            throw new InvalidOperationException("Cannot write off paid debt");

        Status = DebtStatus.WrittenOff;
    }

    /// <summary>
    /// Calculates remaining amount: Amount - PaidAmount
    /// </summary>
    public void CalculateRemainingAmount()
    {
        RemainingAmount = Amount - PaidAmount;
    }

    /// <summary>
    /// Validates debt constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DebtNumber))
            throw new ArgumentException("Debt number is required", nameof(DebtNumber));

        if (Amount <= 0)
            throw new ArgumentException("Amount must be > 0", nameof(Amount));

        if (Status == DebtStatus.Paid && !PaidDate.HasValue)
            throw new InvalidOperationException("Paid date is required when status is Paid");

        var expectedRemaining = Amount - PaidAmount;
        if (Math.Abs(RemainingAmount - expectedRemaining) > 0.01m)
            throw new InvalidOperationException("RemainingAmount must equal Amount - PaidAmount");
    }
}

public enum DebtStatus
{
    Pending = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Overdue = 3,
    WrittenOff = 4
}
