using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Customers;

public class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DebtNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.RemainingAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.DebtNumber).IsUnique();
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => new { e.Status, e.DueDate });

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Debts)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
