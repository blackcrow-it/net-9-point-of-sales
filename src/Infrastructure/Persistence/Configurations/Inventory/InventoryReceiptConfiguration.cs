using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryReceiptConfiguration : IEntityTypeConfiguration<InventoryReceipt>
{
    public void Configure(EntityTypeBuilder<InventoryReceipt> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SupplierInvoiceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.ReceivedBy)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.ReceiptNumber).IsUnique();
        builder.HasIndex(e => new { e.StoreId, e.ReceiptDate });
        builder.HasIndex(e => e.SupplierId);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany()
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Receipt)
            .HasForeignKey(i => i.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
