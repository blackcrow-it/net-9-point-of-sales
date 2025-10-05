using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryIssueItemConfiguration : IEntityTypeConfiguration<InventoryIssueItem>
{
    public void Configure(EntityTypeBuilder<InventoryIssueItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.UnitCost)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.IssueId);
        builder.HasIndex(e => new { e.IssueId, e.LineNumber }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Issue)
            .WithMany(i => i.Items)
            .HasForeignKey(e => e.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProductVariant)
            .WithMany()
            .HasForeignKey(e => e.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
