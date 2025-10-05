using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Sales;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ShiftNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.OpeningCash)
            .HasPrecision(18, 2);

        builder.Property(e => e.ClosingCash)
            .HasPrecision(18, 2);

        builder.Property(e => e.ExpectedCash)
            .HasPrecision(18, 2);

        builder.Property(e => e.CashDifference)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalSales)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.ShiftNumber).IsUnique();
        builder.HasIndex(e => new { e.CashierId, e.Status });
        builder.HasIndex(e => new { e.StoreId, e.StartTime });

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Cashier)
            .WithMany()
            .HasForeignKey(e => e.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Orders)
            .WithOne(o => o.Shift)
            .HasForeignKey(o => o.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
