using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .HasMaxLength(255);

        builder.Property(e => e.Barcode)
            .HasMaxLength(100);

        builder.Property(e => e.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.RetailPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.WholesalePrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.Attributes)
            .HasColumnType("jsonb");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.SKU).IsUnique();
        builder.HasIndex(e => e.Barcode).IsUnique().HasFilter("\"Barcode\" IS NOT NULL");
        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasOne(e => e.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.InventoryLevels)
            .WithOne(il => il.ProductVariant)
            .HasForeignKey(il => il.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
