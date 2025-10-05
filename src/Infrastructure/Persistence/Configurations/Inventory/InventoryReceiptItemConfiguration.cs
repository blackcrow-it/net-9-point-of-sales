using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryReceiptItemConfiguration : IEntityTypeConfiguration<InventoryReceiptItem>
{
    public void Configure(EntityTypeBuilder<InventoryReceiptItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.UnitCost)
            .HasPrecision(18, 2);

        builder.Property(e => e.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.ReceiptId);
        builder.HasIndex(e => new { e.ReceiptId, e.LineNumber }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Receipt)
            .WithMany(r => r.Items)
            .HasForeignKey(e => e.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProductVariant)
            .WithMany()
            .HasForeignKey(e => e.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
