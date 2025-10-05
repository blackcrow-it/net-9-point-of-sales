using Domain.Common;

namespace Domain.Entities.Sales;

/// <summary>
/// Represents a cashier work session
/// </summary>
public class Shift : BaseEntity
{
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }

    /// <summary>
    /// Shift number with format: SFT-YYYYMMDD-XXXX
    /// </summary>
    public string ShiftNumber { get; set; } = string.Empty;

    public ShiftStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public decimal OpeningCash { get; set; }
    public decimal ClosingCash { get; set; }
    public decimal ExpectedCash { get; set; }

    /// <summary>
    /// Cash difference: ClosingCash - ExpectedCash
    /// </summary>
    public decimal CashDifference { get; set; }

    public decimal TotalSales { get; set; }
    public int TotalTransactions { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Store.Store Store { get; set; } = null!;
    public Employees.User Cashier { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Closes the shift and calculates cash difference
    /// </summary>
    public void CloseShift(decimal closingCash, decimal expectedCash)
    {
        if (Status == ShiftStatus.Closed)
            throw new InvalidOperationException("Shift is already closed");

        Status = ShiftStatus.Closed;
        EndTime = DateTime.UtcNow;
        ClosingCash = closingCash;
        ExpectedCash = expectedCash;
        CalculateCashDifference();
    }

    /// <summary>
    /// Calculates the cash difference: ClosingCash - ExpectedCash
    /// </summary>
    public void CalculateCashDifference()
    {
        CashDifference = ClosingCash - ExpectedCash;
    }

    /// <summary>
    /// Validates shift constraints
    /// </summary>
    public void Validate()
    {
        if (OpeningCash < 0)
            throw new ArgumentException("Opening cash must be >= 0", nameof(OpeningCash));

        if (Status == ShiftStatus.Closed && !EndTime.HasValue)
            throw new InvalidOperationException("End time is required when shift is closed");
    }
}

public enum ShiftStatus
{
    Open = 0,
    Closed = 1
}
