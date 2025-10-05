using Domain.Entities.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Employees;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Resource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => new { e.RoleId, e.Resource, e.Action }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
