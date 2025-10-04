using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Main database context for the Point of Sale application
/// Implements audit trail, soft delete, and multi-tenancy patterns
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    // DbSets will be added here as entities are created
    // Example: public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Create the query filter expression: e => !e.IsDeleted
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var propertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = System.Linq.Expressions.Expression.Call(
                    propertyMethod,
                    parameter,
                    System.Linq.Expressions.Expression.Constant(nameof(BaseEntity.IsDeleted)));
                var notDeleted = System.Linq.Expressions.Expression.Not(isDeletedProperty);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // Multi-tenancy filter can be applied here when needed
        // Example filter for entities with StoreId:
        // if (_currentUserService?.StoreId != null)
        // {
        //     modelBuilder.Entity<Order>().HasQueryFilter(o => o.StoreId == _currentUserService.StoreId);
        // }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit fields are handled by AuditableEntityInterceptor
        return await base.SaveChangesAsync(cancellationToken);
    }
}
