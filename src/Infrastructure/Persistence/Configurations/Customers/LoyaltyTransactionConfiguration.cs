using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Customers;

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Points)
            .HasPrecision(10, 2);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => new { e.CustomerId, e.TransactionDate });

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany(c => c.LoyaltyTransactions)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
