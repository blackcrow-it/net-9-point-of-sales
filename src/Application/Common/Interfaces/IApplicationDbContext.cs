using Domain.Entities.Sales;
using Domain.Entities.Inventory;
using Domain.Entities.Customers;
using Domain.Entities.Employees;
using Domain.Entities.Store;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

/// <summary>
/// Database context interface for the application
/// Provides access to DbSets and database operations
/// </summary>
public interface IApplicationDbContext
{
    // Sales Module
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }

    // Inventory Module
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<InventoryLevel> InventoryLevels { get; }
    DbSet<InventoryReceipt> InventoryReceipts { get; }
    DbSet<InventoryReceiptItem> InventoryReceiptItems { get; }
    DbSet<InventoryIssue> InventoryIssues { get; }
    DbSet<InventoryIssueItem> InventoryIssueItems { get; }
    DbSet<Stocktake> Stocktakes { get; }
    DbSet<StocktakeItem> StocktakeItems { get; }

    // Customer Module
    DbSet<Customer> Customers { get; }
    DbSet<CustomerGroup> CustomerGroups { get; }
    DbSet<LoyaltyTransaction> LoyaltyTransactions { get; }
    DbSet<Debt> Debts { get; }

    // Employee Module
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Commission> Commissions { get; }

    // Store Module
    DbSet<Store> Stores { get; }
    DbSet<Supplier> Suppliers { get; }

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
