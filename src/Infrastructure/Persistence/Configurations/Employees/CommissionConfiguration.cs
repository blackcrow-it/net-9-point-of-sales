using Domain.Entities.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Employees;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.CommissionRate)
            .HasPrecision(5, 2);

        builder.Property(e => e.CommissionAmount)
            .HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(e => new { e.UserId, e.OrderId }).IsUnique();
        builder.HasIndex(e => new { e.UserId, e.Status });
        builder.HasIndex(e => e.CalculatedDate);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany(u => u.Commissions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Order)
            .WithMany()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
