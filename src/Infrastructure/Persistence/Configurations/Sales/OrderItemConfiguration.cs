using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Sales;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductSKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.VariantName)
            .HasMaxLength(255);

        builder.Property(e => e.Quantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.LineTotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.ProductVariantId);
        builder.HasIndex(e => new { e.OrderId, e.LineNumber }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProductVariant)
            .WithMany()
            .HasForeignKey(e => e.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
