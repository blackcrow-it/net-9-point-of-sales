using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Sales;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.VoidReason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.OrderNumber).IsUnique();
        builder.HasIndex(e => new { e.StoreId, e.CreatedAt });
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.ShiftId);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Cashier)
            .WithMany()
            .HasForeignKey(e => e.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shift)
            .WithMany(s => s.Orders)
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
