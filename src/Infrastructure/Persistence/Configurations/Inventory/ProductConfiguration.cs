using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.RetailPrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.WholesalePrice)
            .HasPrecision(18, 2);

        builder.Property(e => e.ReorderLevel)
            .HasPrecision(10, 3);

        builder.Property(e => e.ReorderQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Tags)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.SKU).IsUnique();
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.BrandId);
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(e => e.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
