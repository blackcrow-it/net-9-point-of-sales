using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryIssueConfiguration : IEntityTypeConfiguration<InventoryIssue>
{
    public void Configure(EntityTypeBuilder<InventoryIssue> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.IssueNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.IssueNumber).IsUnique();
        builder.HasIndex(e => new { e.StoreId, e.IssueDate });
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestinationStore)
            .WithMany()
            .HasForeignKey(e => e.DestinationStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Issue)
            .HasForeignKey(i => i.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
