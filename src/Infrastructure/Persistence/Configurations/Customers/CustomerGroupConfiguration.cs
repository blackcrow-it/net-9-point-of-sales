using Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Customers;

public class CustomerGroupConfiguration : IEntityTypeConfiguration<CustomerGroup>
{
    public void Configure(EntityTypeBuilder<CustomerGroup> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.DiscountPercentage)
            .HasPrecision(5, 2);

        builder.Property(e => e.LoyaltyPointsMultiplier)
            .HasPrecision(5, 2)
            .HasDefaultValue(1.0m);

        // Indexes
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasMany(e => e.Customers)
            .WithOne(c => c.CustomerGroup)
            .HasForeignKey(c => c.CustomerGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
