using Domain.Entities.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Stores;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ContactPerson)
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

        builder.Property(e => e.TaxCode)
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.IsActive);
    }
}
