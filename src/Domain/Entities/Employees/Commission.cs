using Domain.Common;

namespace Domain.Entities.Employees;

/// <summary>
/// Employee sales commissions
/// </summary>
public class Commission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }

    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; }

    /// <summary>
    /// Commission amount: OrderAmount * (CommissionRate / 100)
    /// </summary>
    public decimal CommissionAmount { get; set; }

    public DateTime CalculatedDate { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime? PaidDate { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Sales.Order Order { get; set; } = null!;

    /// <summary>
    /// Calculates commission amount: OrderAmount * (CommissionRate / 100)
    /// </summary>
    public void CalculateCommissionAmount()
    {
        CommissionAmount = OrderAmount * (CommissionRate / 100);
    }

    /// <summary>
    /// Validates commission constraints
    /// </summary>
    public void Validate()
    {
        var expectedAmount = OrderAmount * (CommissionRate / 100);
        if (Math.Abs(CommissionAmount - expectedAmount) > 0.01m)
            throw new InvalidOperationException("CommissionAmount must equal OrderAmount * (CommissionRate / 100)");

        if (Status == CommissionStatus.Paid && !PaidDate.HasValue)
            throw new InvalidOperationException("Paid date is required when status is Paid");
    }
}

public enum CommissionStatus
{
    Pending = 0,
    Approved = 1,
    Paid = 2,
    Cancelled = 3
}
