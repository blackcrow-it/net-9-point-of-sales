using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CustomerNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.District)
            .HasMaxLength(100);

        builder.Property(e => e.Ward)
            .HasMaxLength(100);

        builder.Property(e => e.LoyaltyPoints)
            .HasPrecision(10, 2);

        builder.Property(e => e.TotalSpent)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.CustomerNumber).IsUnique();
        builder.HasIndex(e => e.Phone);
        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.CustomerGroupId);
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasOne(e => e.CustomerGroup)
            .WithMany(cg => cg.Customers)
            .HasForeignKey(e => e.CustomerGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.LoyaltyTransactions)
            .WithOne(lt => lt.Customer)
            .HasForeignKey(lt => lt.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Debts)
            .WithOne(d => d.Customer)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
