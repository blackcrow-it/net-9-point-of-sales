using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class StocktakeItemConfiguration : IEntityTypeConfiguration<StocktakeItem>
{
    public void Configure(EntityTypeBuilder<StocktakeItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.SystemQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.CountedQuantity)
            .HasPrecision(10, 3);

        builder.Property(e => e.Variance)
            .HasPrecision(10, 3);

        builder.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => new { e.StocktakeId, e.ProductVariantId }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Stocktake)
            .WithMany(s => s.Items)
            .HasForeignKey(e => e.StocktakeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ProductVariant)
            .WithMany()
            .HasForeignKey(e => e.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
