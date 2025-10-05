using Domain.Common;

namespace Domain.Entities.Inventory;

/// <summary>
/// Goods receipt from suppliers
/// </summary>
public class InventoryReceipt : BaseEntity
{
    /// <summary>
    /// Receipt number with format: GRN-YYYYMMDD-XXXX
    /// </summary>
    public string ReceiptNumber { get; set; } = string.Empty;

    public Guid StoreId { get; set; }
    public Guid? SupplierId { get; set; }

    public ReceiptStatus Status { get; set; }
    public DateTime ReceiptDate { get; set; }

    public string? SupplierInvoiceNumber { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }

    // Navigation Properties
    public Store.Store Store { get; set; } = null!;
    public Store.Supplier? Supplier { get; set; }
    public ICollection<InventoryReceiptItem> Items { get; set; } = new List<InventoryReceiptItem>();

    /// <summary>
    /// Completes the receipt
    /// </summary>
    public void Complete()
    {
        if (Status != ReceiptStatus.Draft)
            throw new InvalidOperationException($"Cannot complete receipt with status {Status}");

        Status = ReceiptStatus.Completed;
    }

    /// <summary>
    /// Cancels the receipt
    /// </summary>
    public void Cancel()
    {
        if (Status == ReceiptStatus.Cancelled)
            throw new InvalidOperationException("Receipt is already cancelled");

        if (Status == ReceiptStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed receipt");

        Status = ReceiptStatus.Cancelled;
    }

    /// <summary>
    /// Validates receipt constraints
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ReceiptNumber))
            throw new ArgumentException("Receipt number is required", nameof(ReceiptNumber));

        if (TotalAmount < 0)
            throw new ArgumentException("Total amount must be >= 0", nameof(TotalAmount));

        if (ReceiptDate > DateTime.UtcNow)
            throw new ArgumentException("Receipt date cannot be in future", nameof(ReceiptDate));
    }
}

public enum ReceiptStatus
{
    Draft = 0,
    Completed = 1,
    Cancelled = 2
}
