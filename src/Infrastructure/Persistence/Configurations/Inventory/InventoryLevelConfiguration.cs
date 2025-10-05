using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class InventoryLevelConfiguration : IEntityTypeConfiguration<InventoryLevel>
{
    public void Configure(EntityTypeBuilder<InventoryLevel> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AvailableQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.ReservedQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.OnHandQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.LastCountedBy)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => new { e.ProductVariantId, e.StoreId }).IsUnique();
        builder.HasIndex(e => e.StoreId);

        // Relationships
        builder.HasOne(e => e.ProductVariant)
            .WithMany(pv => pv.InventoryLevels)
            .HasForeignKey(e => e.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
