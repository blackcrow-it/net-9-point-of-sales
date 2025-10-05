using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Sales;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.ProcessedBy)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.PaymentNumber).IsUnique();
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ReferenceNumber);

        // Relationships
        builder.HasOne(e => e.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PaymentMethod)
            .WithMany()
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
