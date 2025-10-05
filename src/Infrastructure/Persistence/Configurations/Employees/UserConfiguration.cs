using Domain.Entities.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Employees;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.RefreshToken)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        builder.HasIndex(e => e.StoreId);
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Commissions)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
