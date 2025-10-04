# Data Model Specification

## Overview

This document defines all 22 entities for the POS backend system using Clean Architecture principles. All entities include audit fields and support soft delete patterns. Multi-tenancy is implemented via StoreId foreign key where applicable.

## Audit Fields (Base Entity)

All entities inherit from `BaseEntity` with the following fields:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } // UserId
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

---

## Sales Module

### 1. Order

**Purpose**: Represents a sales transaction at POS

**Fields**:
```csharp
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } // Format: ORD-YYYYMMDD-XXXX
    public Guid StoreId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid CashierId { get; set; } // User who created order
    public Guid ShiftId { get; set; }
    public OrderStatus Status { get; set; } // Draft, Completed, Voided, Returned
    public OrderType Type { get; set; } // Sale, Return, Exchange

    public decimal Subtotal { get; set; } // Precision: decimal(18,2)
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }

    // Navigation Properties
    public Store Store { get; set; }
    public Customer? Customer { get; set; }
    public User Cashier { get; set; }
    public Shift Shift { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Payment> Payments { get; set; }
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
```

**Validation Rules**:
- OrderNumber must be unique per store
- Subtotal must be >= 0
- TotalAmount = Subtotal + TaxAmount - DiscountAmount
- CompletedAt required when Status = Completed
- VoidReason required when Status = Voided

**State Transitions**:
- Draft -> Completed, OnHold, Voided
- OnHold -> Draft, Completed, Voided
- Completed -> Returned (creates new Return order)
- Voided (terminal state)

**Indexes**:
```csharp
entity.HasIndex(e => e.OrderNumber).IsUnique();
entity.HasIndex(e => new { e.StoreId, e.CreatedAt });
entity.HasIndex(e => e.CustomerId);
entity.HasIndex(e => e.ShiftId);
entity.HasIndex(e => e.Status);
```

**EF Core Configuration**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Subtotal)
            .HasPrecision(18, 2);
        entity.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);
        entity.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);
        entity.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        entity.Property(e => e.Notes)
            .HasMaxLength(1000);

        entity.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(e => e.Cashier)
            .WithMany()
            .HasForeignKey(e => e.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Shift)
            .WithMany(s => s.Orders)
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    });
}
```

---

### 2. OrderItem

**Purpose**: Line items in an order

**Fields**:
```csharp
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int LineNumber { get; set; } // Sequential within order

    public string ProductSKU { get; set; }
    public string ProductName { get; set; }
    public string? VariantName { get; set; }

    public decimal Quantity { get; set; } // Precision: decimal(10,3)
    public string Unit { get; set; } // "pcs", "kg", "m", etc.

    public decimal UnitPrice { get; set; } // Precision: decimal(18,2)
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; } // Percentage: decimal(5,2)
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Order Order { get; set; }
    public ProductVariant ProductVariant { get; set; }
}
```

**Validation Rules**:
- Quantity must be > 0
- UnitPrice must be >= 0
- LineTotal = (Quantity * UnitPrice - DiscountAmount) + TaxAmount
- LineNumber must be unique within Order

**Indexes**:
```csharp
entity.HasIndex(e => e.OrderId);
entity.HasIndex(e => e.ProductVariantId);
entity.HasIndex(e => new { e.OrderId, e.LineNumber }).IsUnique();
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<OrderItem>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.ProductSKU)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.ProductName)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.VariantName)
        .HasMaxLength(255);

    entity.Property(e => e.Quantity)
        .HasPrecision(10, 3);

    entity.Property(e => e.Unit)
        .IsRequired()
        .HasMaxLength(20);

    entity.Property(e => e.UnitPrice)
        .HasPrecision(18, 2);
    entity.Property(e => e.DiscountAmount)
        .HasPrecision(18, 2);
    entity.Property(e => e.TaxRate)
        .HasPrecision(5, 2);
    entity.Property(e => e.TaxAmount)
        .HasPrecision(18, 2);
    entity.Property(e => e.LineTotal)
        .HasPrecision(18, 2);

    entity.HasOne(e => e.Order)
        .WithMany(o => o.OrderItems)
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ProductVariant)
        .WithMany()
        .HasForeignKey(e => e.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 3. Payment

**Purpose**: Tracks payment transactions for orders

**Fields**:
```csharp
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentNumber { get; set; } // Format: PAY-YYYYMMDD-XXXX

    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; } // Precision: decimal(18,2)
    public string? ReferenceNumber { get; set; } // External transaction ID

    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Order Order { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}
```

**Validation Rules**:
- Amount must be > 0
- ProcessedAt required when Status = Completed
- ReferenceNumber required for electronic payments

**Indexes**:
```csharp
entity.HasIndex(e => e.PaymentNumber).IsUnique();
entity.HasIndex(e => e.OrderId);
entity.HasIndex(e => e.Status);
entity.HasIndex(e => e.ReferenceNumber);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Payment>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.PaymentNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Amount)
        .HasPrecision(18, 2);

    entity.Property(e => e.ReferenceNumber)
        .HasMaxLength(100);

    entity.HasOne(e => e.Order)
        .WithMany(o => o.Payments)
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.PaymentMethod)
        .WithMany()
        .HasForeignKey(e => e.PaymentMethodId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 4. Shift

**Purpose**: Tracks cashier work sessions

**Fields**:
```csharp
public class Shift : BaseEntity
{
    public Guid StoreId { get; set; }
    public Guid CashierId { get; set; }
    public string ShiftNumber { get; set; } // Format: SFT-YYYYMMDD-XXXX

    public ShiftStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public decimal OpeningCash { get; set; } // Precision: decimal(18,2)
    public decimal ClosingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal CashDifference { get; set; } // ClosingCash - ExpectedCash

    public decimal TotalSales { get; set; }
    public int TotalTransactions { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Store Store { get; set; }
    public User Cashier { get; set; }
    public ICollection<Order> Orders { get; set; }
}

public enum ShiftStatus
{
    Open = 0,
    Closed = 1
}
```

**Validation Rules**:
- StartTime required
- EndTime required when Status = Closed
- OpeningCash must be >= 0
- Only one open shift per cashier at a time

**State Transitions**:
- Open -> Closed (irreversible)

**Indexes**:
```csharp
entity.HasIndex(e => e.ShiftNumber).IsUnique();
entity.HasIndex(e => new { e.CashierId, e.Status });
entity.HasIndex(e => new { e.StoreId, e.StartTime });
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Shift>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.ShiftNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.OpeningCash)
        .HasPrecision(18, 2);
    entity.Property(e => e.ClosingCash)
        .HasPrecision(18, 2);
    entity.Property(e => e.ExpectedCash)
        .HasPrecision(18, 2);
    entity.Property(e => e.CashDifference)
        .HasPrecision(18, 2);
    entity.Property(e => e.TotalSales)
        .HasPrecision(18, 2);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.Cashier)
        .WithMany()
        .HasForeignKey(e => e.CashierId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 5. PaymentMethod

**Purpose**: Configuration for available payment methods

**Fields**:
```csharp
public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } // "Cash", "Card", "VNPay", "MoMo", etc.
    public string Code { get; set; } // "CASH", "CARD", "VNPAY", "MOMO"
    public PaymentMethodType Type { get; set; }

    public bool IsActive { get; set; }
    public bool RequiresReference { get; set; } // External transaction ID

    public string? Description { get; set; }
    public string? Configuration { get; set; } // JSON config for gateway settings

    public int DisplayOrder { get; set; }
}

public enum PaymentMethodType
{
    Cash = 0,
    Card = 1,
    EWallet = 2,
    BankTransfer = 3,
    Other = 99
}
```

**Validation Rules**:
- Code must be unique
- Name required
- DisplayOrder must be >= 0

**Indexes**:
```csharp
entity.HasIndex(e => e.Code).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<PaymentMethod>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Description)
        .HasMaxLength(500);

    entity.Property(e => e.Configuration)
        .HasColumnType("jsonb"); // PostgreSQL JSON
});
```

---

## Inventory Module

### 6. Product

**Purpose**: Master product data

**Fields**:
```csharp
public class Product : BaseEntity
{
    public string SKU { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }

    public ProductType Type { get; set; } // Single, Variable
    public string Unit { get; set; } // Base unit: "pcs", "kg", etc.

    public decimal? CostPrice { get; set; } // Precision: decimal(18,2)
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }

    public bool TrackInventory { get; set; }
    public decimal? ReorderLevel { get; set; } // Precision: decimal(10,3)
    public decimal? ReorderQuantity { get; set; }

    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; } // Comma-separated

    // Navigation Properties
    public Category? Category { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductVariant> Variants { get; set; }
}

public enum ProductType
{
    Single = 0,    // No variants
    Variable = 1   // Has variants (e.g., size, color)
}
```

**Validation Rules**:
- SKU must be unique
- Name required
- CostPrice, RetailPrice must be >= 0 if provided
- ReorderLevel, ReorderQuantity required if TrackInventory = true

**Indexes**:
```csharp
entity.HasIndex(e => e.SKU).IsUnique();
entity.HasIndex(e => e.Name);
entity.HasIndex(e => e.CategoryId);
entity.HasIndex(e => e.BrandId);
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.SKU)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.Description)
        .HasMaxLength(2000);

    entity.Property(e => e.Unit)
        .IsRequired()
        .HasMaxLength(20);

    entity.Property(e => e.CostPrice)
        .HasPrecision(18, 2);
    entity.Property(e => e.RetailPrice)
        .HasPrecision(18, 2);
    entity.Property(e => e.WholesalePrice)
        .HasPrecision(18, 2);

    entity.Property(e => e.ReorderLevel)
        .HasPrecision(10, 3);
    entity.Property(e => e.ReorderQuantity)
        .HasPrecision(10, 3);

    entity.Property(e => e.ImageUrl)
        .HasMaxLength(500);

    entity.HasOne(e => e.Category)
        .WithMany(c => c.Products)
        .HasForeignKey(e => e.CategoryId)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.Brand)
        .WithMany(b => b.Products)
        .HasForeignKey(e => e.BrandId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

---

### 7. ProductVariant

**Purpose**: Product variations (size, color, etc.)

**Fields**:
```csharp
public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string SKU { get; set; }
    public string? Name { get; set; } // e.g., "Large - Red"

    public string? Barcode { get; set; }

    public decimal? CostPrice { get; set; } // Precision: decimal(18,2)
    public decimal? RetailPrice { get; set; }
    public decimal? WholesalePrice { get; set; }

    public bool IsActive { get; set; }
    public bool IsDefault { get; set; } // Default variant for product

    public string? Attributes { get; set; } // JSON: {"size": "L", "color": "Red"}
    public string? ImageUrl { get; set; }

    // Navigation Properties
    public Product Product { get; set; }
    public ICollection<InventoryLevel> InventoryLevels { get; set; }
}
```

**Validation Rules**:
- SKU must be unique
- Barcode must be unique if provided
- Only one default variant per product
- Prices inherit from Product if not specified

**Indexes**:
```csharp
entity.HasIndex(e => e.SKU).IsUnique();
entity.HasIndex(e => e.Barcode).IsUnique().HasFilter("Barcode IS NOT NULL");
entity.HasIndex(e => e.ProductId);
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<ProductVariant>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.SKU)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Name)
        .HasMaxLength(255);

    entity.Property(e => e.Barcode)
        .HasMaxLength(100);

    entity.Property(e => e.CostPrice)
        .HasPrecision(18, 2);
    entity.Property(e => e.RetailPrice)
        .HasPrecision(18, 2);
    entity.Property(e => e.WholesalePrice)
        .HasPrecision(18, 2);

    entity.Property(e => e.Attributes)
        .HasColumnType("jsonb");

    entity.Property(e => e.ImageUrl)
        .HasMaxLength(500);

    entity.HasOne(e => e.Product)
        .WithMany(p => p.Variants)
        .HasForeignKey(e => e.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

---

### 8. Category

**Purpose**: Product categorization hierarchy

**Fields**:
```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    public string? ImageUrl { get; set; }

    // Navigation Properties
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; }
    public ICollection<Product> Products { get; set; }
}
```

**Validation Rules**:
- Name must be unique within same ParentId
- No circular references in hierarchy
- DisplayOrder must be >= 0

**Indexes**:
```csharp
entity.HasIndex(e => new { e.ParentId, e.Name }).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Category>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Description)
        .HasMaxLength(500);

    entity.Property(e => e.ImageUrl)
        .HasMaxLength(500);

    entity.HasOne(e => e.Parent)
        .WithMany(c => c.Children)
        .HasForeignKey(e => e.ParentId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 9. Brand

**Purpose**: Product brands/manufacturers

**Fields**:
```csharp
public class Brand : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }

    // Navigation Properties
    public ICollection<Product> Products { get; set; }
}
```

**Validation Rules**:
- Name must be unique
- Website must be valid URL if provided

**Indexes**:
```csharp
entity.HasIndex(e => e.Name).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Brand>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Description)
        .HasMaxLength(500);

    entity.Property(e => e.LogoUrl)
        .HasMaxLength(500);

    entity.Property(e => e.Website)
        .HasMaxLength(255);
});
```

---

### 10. InventoryLevel

**Purpose**: Current stock levels per store/warehouse

**Fields**:
```csharp
public class InventoryLevel : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public Guid StoreId { get; set; }

    public decimal AvailableQuantity { get; set; } // Precision: decimal(10,3)
    public decimal ReservedQuantity { get; set; }  // On hold for orders
    public decimal OnHandQuantity { get; set; }    // Physical count

    public DateTime? LastCountedAt { get; set; }
    public string? LastCountedBy { get; set; }

    // Navigation Properties
    public ProductVariant ProductVariant { get; set; }
    public Store Store { get; set; }
}
```

**Validation Rules**:
- OnHandQuantity = AvailableQuantity + ReservedQuantity
- Unique constraint on (ProductVariantId, StoreId)

**Indexes**:
```csharp
entity.HasIndex(e => new { e.ProductVariantId, e.StoreId }).IsUnique();
entity.HasIndex(e => e.StoreId);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<InventoryLevel>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.AvailableQuantity)
        .HasPrecision(10, 3);
    entity.Property(e => e.ReservedQuantity)
        .HasPrecision(10, 3);
    entity.Property(e => e.OnHandQuantity)
        .HasPrecision(10, 3);

    entity.HasOne(e => e.ProductVariant)
        .WithMany(pv => pv.InventoryLevels)
        .HasForeignKey(e => e.ProductVariantId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 11. InventoryReceipt

**Purpose**: Goods receipt from suppliers

**Fields**:
```csharp
public class InventoryReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } // Format: GRN-YYYYMMDD-XXXX
    public Guid StoreId { get; set; }
    public Guid? SupplierId { get; set; }

    public ReceiptStatus Status { get; set; }
    public DateTime ReceiptDate { get; set; }

    public string? SupplierInvoiceNumber { get; set; }
    public decimal TotalAmount { get; set; } // Precision: decimal(18,2)

    public string? Notes { get; set; }
    public string? ReceivedBy { get; set; }

    // Navigation Properties
    public Store Store { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<InventoryReceiptItem> Items { get; set; }
}

public enum ReceiptStatus
{
    Draft = 0,
    Completed = 1,
    Cancelled = 2
}
```

**Validation Rules**:
- ReceiptNumber must be unique
- TotalAmount must be >= 0
- ReceiptDate cannot be in future

**State Transitions**:
- Draft -> Completed, Cancelled
- Completed, Cancelled (terminal states)

**Indexes**:
```csharp
entity.HasIndex(e => e.ReceiptNumber).IsUnique();
entity.HasIndex(e => new { e.StoreId, e.ReceiptDate });
entity.HasIndex(e => e.SupplierId);
entity.HasIndex(e => e.Status);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<InventoryReceipt>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.ReceiptNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.SupplierInvoiceNumber)
        .HasMaxLength(100);

    entity.Property(e => e.TotalAmount)
        .HasPrecision(18, 2);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.Supplier)
        .WithMany()
        .HasForeignKey(e => e.SupplierId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

---

### 12. InventoryReceiptItem

**Purpose**: Line items in goods receipt

**Fields**:
```csharp
public class InventoryReceiptItem : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int LineNumber { get; set; }

    public decimal Quantity { get; set; } // Precision: decimal(10,3)
    public string Unit { get; set; }

    public decimal UnitCost { get; set; } // Precision: decimal(18,2)
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public InventoryReceipt Receipt { get; set; }
    public ProductVariant ProductVariant { get; set; }
}
```

**Validation Rules**:
- Quantity must be > 0
- UnitCost must be >= 0
- LineTotal = Quantity * UnitCost
- LineNumber unique within Receipt

**Indexes**:
```csharp
entity.HasIndex(e => e.ReceiptId);
entity.HasIndex(e => new { e.ReceiptId, e.LineNumber }).IsUnique();
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<InventoryReceiptItem>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Quantity)
        .HasPrecision(10, 3);

    entity.Property(e => e.Unit)
        .IsRequired()
        .HasMaxLength(20);

    entity.Property(e => e.UnitCost)
        .HasPrecision(18, 2);
    entity.Property(e => e.LineTotal)
        .HasPrecision(18, 2);

    entity.HasOne(e => e.Receipt)
        .WithMany(r => r.Items)
        .HasForeignKey(e => e.ReceiptId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ProductVariant)
        .WithMany()
        .HasForeignKey(e => e.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 13. InventoryIssue

**Purpose**: Inventory adjustments (damage, theft, transfer)

**Fields**:
```csharp
public class InventoryIssue : BaseEntity
{
    public string IssueNumber { get; set; } // Format: ISS-YYYYMMDD-XXXX
    public Guid StoreId { get; set; }

    public IssueType Type { get; set; }
    public IssueStatus Status { get; set; }

    public DateTime IssueDate { get; set; }

    public Guid? DestinationStoreId { get; set; } // For transfers

    public string Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public Store Store { get; set; }
    public Store? DestinationStore { get; set; }
    public ICollection<InventoryIssueItem> Items { get; set; }
}

public enum IssueType
{
    Adjustment = 0,  // Manual adjustment
    Damage = 1,      // Damaged goods
    Loss = 2,        // Lost/stolen
    Transfer = 3,    // Transfer to another store
    Return = 4       // Return to supplier
}

public enum IssueStatus
{
    Draft = 0,
    Completed = 1,
    Cancelled = 2
}
```

**Validation Rules**:
- IssueNumber must be unique
- DestinationStoreId required when Type = Transfer
- DestinationStoreId must differ from StoreId

**State Transitions**:
- Draft -> Completed, Cancelled
- Completed, Cancelled (terminal states)

**Indexes**:
```csharp
entity.HasIndex(e => e.IssueNumber).IsUnique();
entity.HasIndex(e => new { e.StoreId, e.IssueDate });
entity.HasIndex(e => e.Type);
entity.HasIndex(e => e.Status);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<InventoryIssue>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.IssueNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Reason)
        .IsRequired()
        .HasMaxLength(500);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(e => e.DestinationStore)
        .WithMany()
        .HasForeignKey(e => e.DestinationStoreId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 14. InventoryIssueItem

**Purpose**: Line items in inventory issues

**Fields**:
```csharp
public class InventoryIssueItem : BaseEntity
{
    public Guid IssueId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int LineNumber { get; set; }

    public decimal Quantity { get; set; } // Precision: decimal(10,3)
    public string Unit { get; set; }

    public decimal? UnitCost { get; set; } // Precision: decimal(18,2)

    public string? Notes { get; set; }

    // Navigation Properties
    public InventoryIssue Issue { get; set; }
    public ProductVariant ProductVariant { get; set; }
}
```

**Validation Rules**:
- Quantity must be > 0
- LineNumber unique within Issue

**Indexes**:
```csharp
entity.HasIndex(e => e.IssueId);
entity.HasIndex(e => new { e.IssueId, e.LineNumber }).IsUnique();
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<InventoryIssueItem>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Quantity)
        .HasPrecision(10, 3);

    entity.Property(e => e.Unit)
        .IsRequired()
        .HasMaxLength(20);

    entity.Property(e => e.UnitCost)
        .HasPrecision(18, 2);

    entity.HasOne(e => e.Issue)
        .WithMany(i => i.Items)
        .HasForeignKey(e => e.IssueId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ProductVariant)
        .WithMany()
        .HasForeignKey(e => e.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 15. Stocktake

**Purpose**: Physical inventory counts

**Fields**:
```csharp
public class Stocktake : BaseEntity
{
    public string StocktakeNumber { get; set; } // Format: STK-YYYYMMDD-XXXX
    public Guid StoreId { get; set; }

    public StocktakeStatus Status { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public string? Notes { get; set; }
    public string? CountedBy { get; set; }

    // Navigation Properties
    public Store Store { get; set; }
    public ICollection<StocktakeItem> Items { get; set; }
}

public enum StocktakeStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
```

**Validation Rules**:
- StocktakeNumber must be unique
- CompletedDate required when Status = Completed
- CompletedDate >= ScheduledDate

**State Transitions**:
- Scheduled -> InProgress, Cancelled
- InProgress -> Completed, Cancelled
- Completed, Cancelled (terminal states)

**Indexes**:
```csharp
entity.HasIndex(e => e.StocktakeNumber).IsUnique();
entity.HasIndex(e => new { e.StoreId, e.ScheduledDate });
entity.HasIndex(e => e.Status);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Stocktake>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.StocktakeNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 16. StocktakeItem

**Purpose**: Counted items in stocktake

**Fields**:
```csharp
public class StocktakeItem : BaseEntity
{
    public Guid StocktakeId { get; set; }
    public Guid ProductVariantId { get; set; }

    public decimal SystemQuantity { get; set; } // Precision: decimal(10,3)
    public decimal CountedQuantity { get; set; }
    public decimal Variance { get; set; } // CountedQuantity - SystemQuantity

    public string Unit { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Stocktake Stocktake { get; set; }
    public ProductVariant ProductVariant { get; set; }
}
```

**Validation Rules**:
- Unique constraint on (StocktakeId, ProductVariantId)
- Variance = CountedQuantity - SystemQuantity

**Indexes**:
```csharp
entity.HasIndex(e => new { e.StocktakeId, e.ProductVariantId }).IsUnique();
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<StocktakeItem>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.SystemQuantity)
        .HasPrecision(10, 3);
    entity.Property(e => e.CountedQuantity)
        .HasPrecision(10, 3);
    entity.Property(e => e.Variance)
        .HasPrecision(10, 3);

    entity.Property(e => e.Unit)
        .IsRequired()
        .HasMaxLength(20);

    entity.HasOne(e => e.Stocktake)
        .WithMany(s => s.Items)
        .HasForeignKey(e => e.StocktakeId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.ProductVariant)
        .WithMany()
        .HasForeignKey(e => e.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

## Customer Module

### 17. Customer

**Purpose**: Customer master data

**Fields**:
```csharp
public class Customer : BaseEntity
{
    public string CustomerNumber { get; set; } // Format: CUS-XXXXXXXX
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public Guid? CustomerGroupId { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public CustomerGender? Gender { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }

    public decimal LoyaltyPoints { get; set; } // Precision: decimal(10,2)
    public decimal TotalSpent { get; set; } // Precision: decimal(18,2)
    public int TotalOrders { get; set; }

    public DateTime? LastPurchaseDate { get; set; }

    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public CustomerGroup? CustomerGroup { get; set; }
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public ICollection<Debt> Debts { get; set; }
}

public enum CustomerGender
{
    Male = 0,
    Female = 1,
    Other = 2
}
```

**Validation Rules**:
- CustomerNumber must be unique
- Phone or Email required (at least one)
- Phone must be valid Vietnamese format (10 digits)
- Email must be valid format

**Indexes**:
```csharp
entity.HasIndex(e => e.CustomerNumber).IsUnique();
entity.HasIndex(e => e.Phone);
entity.HasIndex(e => e.Email);
entity.HasIndex(e => e.Name);
entity.HasIndex(e => e.CustomerGroupId);
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.CustomerNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.Phone)
        .HasMaxLength(20);

    entity.Property(e => e.Email)
        .HasMaxLength(255);

    entity.Property(e => e.Address)
        .HasMaxLength(500);

    entity.Property(e => e.City)
        .HasMaxLength(100);

    entity.Property(e => e.District)
        .HasMaxLength(100);

    entity.Property(e => e.Ward)
        .HasMaxLength(100);

    entity.Property(e => e.LoyaltyPoints)
        .HasPrecision(10, 2);

    entity.Property(e => e.TotalSpent)
        .HasPrecision(18, 2);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);

    entity.HasOne(e => e.CustomerGroup)
        .WithMany(cg => cg.Customers)
        .HasForeignKey(e => e.CustomerGroupId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

---

### 18. CustomerGroup

**Purpose**: Customer segmentation for pricing/promotions

**Fields**:
```csharp
public class CustomerGroup : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public decimal DiscountPercentage { get; set; } // Precision: decimal(5,2)
    public decimal LoyaltyPointsMultiplier { get; set; } // Precision: decimal(5,2), default 1.0

    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<Customer> Customers { get; set; }
}
```

**Validation Rules**:
- Name must be unique
- DiscountPercentage between 0 and 100
- LoyaltyPointsMultiplier must be >= 0

**Indexes**:
```csharp
entity.HasIndex(e => e.Name).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<CustomerGroup>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Description)
        .HasMaxLength(500);

    entity.Property(e => e.DiscountPercentage)
        .HasPrecision(5, 2);

    entity.Property(e => e.LoyaltyPointsMultiplier)
        .HasPrecision(5, 2)
        .HasDefaultValue(1.0m);
});
```

---

### 19. LoyaltyTransaction

**Purpose**: Loyalty points earning/redemption history

**Fields**:
```csharp
public class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    public LoyaltyTransactionType Type { get; set; }
    public decimal Points { get; set; } // Precision: decimal(10,2)

    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; }
    public Order? Order { get; set; }
}

public enum LoyaltyTransactionType
{
    Earned = 0,     // Points earned from purchase
    Redeemed = 1,   // Points used
    Adjusted = 2,   // Manual adjustment
    Expired = 3     // Points expired
}
```

**Validation Rules**:
- Points > 0 for Earned, < 0 for Redeemed/Expired
- OrderId required when Type = Earned or Redeemed

**Indexes**:
```csharp
entity.HasIndex(e => e.CustomerId);
entity.HasIndex(e => e.OrderId);
entity.HasIndex(e => new { e.CustomerId, e.TransactionDate });
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<LoyaltyTransaction>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Points)
        .HasPrecision(10, 2);

    entity.Property(e => e.Description)
        .HasMaxLength(500);

    entity.HasOne(e => e.Customer)
        .WithMany(c => c.LoyaltyTransactions)
        .HasForeignKey(e => e.CustomerId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Order)
        .WithMany()
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

---

### 20. Debt

**Purpose**: Customer account receivables

**Fields**:
```csharp
public class Debt : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    public string DebtNumber { get; set; } // Format: DBT-YYYYMMDD-XXXX
    public DebtStatus Status { get; set; }

    public decimal Amount { get; set; } // Precision: decimal(18,2)
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Customer Customer { get; set; }
    public Order? Order { get; set; }
}

public enum DebtStatus
{
    Pending = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Overdue = 3,
    WrittenOff = 4
}
```

**Validation Rules**:
- DebtNumber must be unique
- Amount must be > 0
- RemainingAmount = Amount - PaidAmount
- PaidDate required when Status = Paid

**State Transitions**:
- Pending -> PartiallyPaid, Paid, Overdue, WrittenOff
- PartiallyPaid -> Paid, Overdue, WrittenOff
- Overdue -> PartiallyPaid, Paid, WrittenOff
- Paid, WrittenOff (terminal states)

**Indexes**:
```csharp
entity.HasIndex(e => e.DebtNumber).IsUnique();
entity.HasIndex(e => e.CustomerId);
entity.HasIndex(e => new { e.Status, e.DueDate });
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Debt>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.DebtNumber)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Amount)
        .HasPrecision(18, 2);
    entity.Property(e => e.PaidAmount)
        .HasPrecision(18, 2);
    entity.Property(e => e.RemainingAmount)
        .HasPrecision(18, 2);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);

    entity.HasOne(e => e.Customer)
        .WithMany(c => c.Debts)
        .HasForeignKey(e => e.CustomerId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Order)
        .WithMany()
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.SetNull);
});
```

---

## Employee Module

### 21. User

**Purpose**: System users (employees)

**Fields**:
```csharp
public class User : BaseEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public Guid? StoreId { get; set; } // Primary store assignment
    public Guid RoleId { get; set; }

    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Navigation Properties
    public Store? Store { get; set; }
    public Role Role { get; set; }
    public ICollection<Commission> Commissions { get; set; }
}
```

**Validation Rules**:
- Username must be unique
- Email must be unique if provided
- PasswordHash required
- Phone must be valid Vietnamese format

**Indexes**:
```csharp
entity.HasIndex(e => e.Username).IsUnique();
entity.HasIndex(e => e.Email).IsUnique().HasFilter("Email IS NOT NULL");
entity.HasIndex(e => e.StoreId);
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<User>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Username)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.PasswordHash)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.FullName)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.Email)
        .HasMaxLength(255);

    entity.Property(e => e.Phone)
        .HasMaxLength(20);

    entity.Property(e => e.RefreshToken)
        .HasMaxLength(500);

    entity.HasOne(e => e.Store)
        .WithMany()
        .HasForeignKey(e => e.StoreId)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.Role)
        .WithMany(r => r.Users)
        .HasForeignKey(e => e.RoleId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

### 22. Role

**Purpose**: User roles with permissions

**Fields**:
```csharp
public class Role : BaseEntity
{
    public string Name { get; set; }
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    // Navigation Properties
    public ICollection<User> Users { get; set; }
    public ICollection<Permission> Permissions { get; set; }
}
```

**Validation Rules**:
- Name must be unique

**Indexes**:
```csharp
entity.HasIndex(e => e.Name).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Role>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Description)
        .HasMaxLength(500);
});
```

---

### 23. Permission

**Purpose**: Granular access control

**Fields**:
```csharp
public class Permission : BaseEntity
{
    public Guid RoleId { get; set; }
    public string Resource { get; set; } // "Orders", "Products", "Reports", etc.
    public string Action { get; set; }   // "View", "Create", "Update", "Delete"

    // Navigation Properties
    public Role Role { get; set; }
}
```

**Validation Rules**:
- Unique constraint on (RoleId, Resource, Action)

**Indexes**:
```csharp
entity.HasIndex(e => new { e.RoleId, e.Resource, e.Action }).IsUnique();
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Permission>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Resource)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Action)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasOne(e => e.Role)
        .WithMany(r => r.Permissions)
        .HasForeignKey(e => e.RoleId)
        .OnDelete(DeleteBehavior.Cascade);
});
```

---

### 24. Commission

**Purpose**: Employee sales commissions

**Fields**:
```csharp
public class Commission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }

    public decimal OrderAmount { get; set; } // Precision: decimal(18,2)
    public decimal CommissionRate { get; set; } // Precision: decimal(5,2)
    public decimal CommissionAmount { get; set; } // Precision: decimal(18,2)

    public DateTime CalculatedDate { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime? PaidDate { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Order Order { get; set; }
}

public enum CommissionStatus
{
    Pending = 0,
    Approved = 1,
    Paid = 2,
    Cancelled = 3
}
```

**Validation Rules**:
- Unique constraint on (UserId, OrderId)
- CommissionAmount = OrderAmount * (CommissionRate / 100)
- PaidDate required when Status = Paid

**Indexes**:
```csharp
entity.HasIndex(e => new { e.UserId, e.OrderId }).IsUnique();
entity.HasIndex(e => new { e.UserId, e.Status });
entity.HasIndex(e => e.CalculatedDate);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Commission>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.OrderAmount)
        .HasPrecision(18, 2);
    entity.Property(e => e.CommissionRate)
        .HasPrecision(5, 2);
    entity.Property(e => e.CommissionAmount)
        .HasPrecision(18, 2);

    entity.HasOne(e => e.User)
        .WithMany(u => u.Commissions)
        .HasForeignKey(e => e.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Order)
        .WithMany()
        .HasForeignKey(e => e.OrderId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

## Store Module

### 25. Store

**Purpose**: Physical store/warehouse locations

**Fields**:
```csharp
public class Store : BaseEntity
{
    public string Code { get; set; } // "STORE001", "WH001"
    public string Name { get; set; }
    public StoreType Type { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Ward { get; set; }

    public string? Phone { get; set; }
    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public string? ManagerId { get; set; } // UserId of store manager
}

public enum StoreType
{
    RetailStore = 0,
    Warehouse = 1,
    Both = 2
}
```

**Validation Rules**:
- Code must be unique
- Name required

**Indexes**:
```csharp
entity.HasIndex(e => e.Code).IsUnique();
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Store>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.Address)
        .HasMaxLength(500);

    entity.Property(e => e.City)
        .HasMaxLength(100);

    entity.Property(e => e.District)
        .HasMaxLength(100);

    entity.Property(e => e.Ward)
        .HasMaxLength(100);

    entity.Property(e => e.Phone)
        .HasMaxLength(20);

    entity.Property(e => e.Email)
        .HasMaxLength(255);

    entity.Property(e => e.ManagerId)
        .HasMaxLength(50);
});
```

---

### 26. Supplier

**Purpose**: Product suppliers

**Fields**:
```csharp
public class Supplier : BaseEntity
{
    public string Code { get; set; }
    public string Name { get; set; }

    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    public string? TaxCode { get; set; }

    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}
```

**Validation Rules**:
- Code must be unique
- Name required

**Indexes**:
```csharp
entity.HasIndex(e => e.Code).IsUnique();
entity.HasIndex(e => e.Name);
entity.HasIndex(e => e.IsActive);
```

**EF Core Configuration**:
```csharp
modelBuilder.Entity<Supplier>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(255);

    entity.Property(e => e.ContactPerson)
        .HasMaxLength(255);

    entity.Property(e => e.Phone)
        .HasMaxLength(20);

    entity.Property(e => e.Email)
        .HasMaxLength(255);

    entity.Property(e => e.Address)
        .HasMaxLength(500);

    entity.Property(e => e.City)
        .HasMaxLength(100);

    entity.Property(e => e.District)
        .HasMaxLength(100);

    entity.Property(e => e.TaxCode)
        .HasMaxLength(50);

    entity.Property(e => e.Notes)
        .HasMaxLength(1000);
});
```

---

## Multi-Tenancy Considerations

All entities that need store-level isolation include `StoreId` foreign key:
- Order
- Shift
- InventoryLevel
- InventoryReceipt
- InventoryIssue
- Stocktake

**Implementation Strategy**:
1. Query filters in DbContext for automatic StoreId filtering
2. Global query filter example:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply to all multi-tenant entities
    modelBuilder.Entity<Order>()
        .HasQueryFilter(e => e.StoreId == CurrentStoreId);

    // Similar for other entities
}
```

3. StoreId populated from authenticated user's context
4. Cross-store operations (transfers) require explicit authorization

---

## Database Indexes Summary

**Performance Critical Indexes**:
1. Order lookup: `(OrderNumber)`, `(StoreId, CreatedAt)`, `(CustomerId)`, `(ShiftId)`
2. Product search: `(SKU)`, `(Name)`, `(Barcode)`, `(CategoryId)`
3. Inventory queries: `(ProductVariantId, StoreId)`, `(StoreId)`
4. Customer lookup: `(CustomerNumber)`, `(Phone)`, `(Email)`
5. User authentication: `(Username)`, `(Email)`

**Composite Indexes** for common query patterns:
- Sales reporting: `(StoreId, CreatedAt, Status)`
- Inventory valuation: `(StoreId, ProductVariantId, CreatedAt)`
- Customer analytics: `(CustomerId, TransactionDate)`

---

## Change Tracking & Audit

All entities include:
- `CreatedAt`, `CreatedBy`: Populated on insert
- `UpdatedAt`, `UpdatedBy`: Updated on every modification
- `IsDeleted`: Soft delete flag (data never physically deleted)

**Implementation via EF Core Interceptors**:

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditFields(DbContext context)
    {
        var entries = context.ChangeTracker.Entries<BaseEntity>();
        var userId = _currentUser.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
            else if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
```

---

## PostgreSQL Specific Features

### JSON Columns
- `PaymentMethod.Configuration`: `jsonb` type for gateway settings
- `ProductVariant.Attributes`: `jsonb` for variant attributes

### Full-Text Search
```sql
CREATE INDEX idx_product_name_fts ON products
USING gin(to_tsvector('english', name));

CREATE INDEX idx_customer_name_fts ON customers
USING gin(to_tsvector('simple', name));
```

### Partitioning for Large Tables
Consider partitioning by date for:
- Order (by CreatedAt monthly)
- LoyaltyTransaction (by TransactionDate yearly)
- Commission (by CalculatedDate monthly)

---

## Entity Relationship Diagram Notes

**One-to-Many Relationships**:
- Store -> Orders, InventoryLevels, Shifts
- Order -> OrderItems, Payments
- Product -> ProductVariants
- Customer -> LoyaltyTransactions, Debts
- User -> Commissions

**Many-to-One Relationships**:
- Order -> Customer, User (Cashier), Shift, Store
- OrderItem -> Order, ProductVariant
- Payment -> Order, PaymentMethod

**Self-Referential**:
- Category.ParentId -> Category

**Complex Relationships**:
- InventoryIssue: Store (source) and DestinationStore (for transfers)
- Role -> Permissions (one-to-many with cascade delete)

---

This data model provides a comprehensive foundation for a production-ready POS system with proper normalization, constraints, and audit capabilities.
