using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Inventory;

public class StocktakeConfiguration : IEntityTypeConfiguration<Stocktake>
{
    public void Configure(EntityTypeBuilder<Stocktake> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StocktakeNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.CountedBy)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.StocktakeNumber).IsUnique();
        builder.HasIndex(e => new { e.StoreId, e.ScheduledDate });
        builder.HasIndex(e => e.Status);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Stocktake)
            .HasForeignKey(i => i.StocktakeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
