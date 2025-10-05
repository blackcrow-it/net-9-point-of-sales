using System.Reflection;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Customers;
using Domain.Entities.Employees;
using Domain.Entities.Store;
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

    // Sales Module
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    // Inventory Module
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<InventoryLevel> InventoryLevels => Set<InventoryLevel>();
    public DbSet<InventoryReceipt> InventoryReceipts => Set<InventoryReceipt>();
    public DbSet<InventoryReceiptItem> InventoryReceiptItems => Set<InventoryReceiptItem>();
    public DbSet<InventoryIssue> InventoryIssues => Set<InventoryIssue>();
    public DbSet<InventoryIssueItem> InventoryIssueItems => Set<InventoryIssueItem>();
    public DbSet<Stocktake> Stocktakes => Set<Stocktake>();
    public DbSet<StocktakeItem> StocktakeItems => Set<StocktakeItem>();

    // Customer Module
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<Debt> Debts => Set<Debt>();

    // Employee Module
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Commission> Commissions => Set<Commission>();

    // Store Module
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

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
