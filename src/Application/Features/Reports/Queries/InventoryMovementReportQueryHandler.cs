using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries;

public class InventoryMovementReportQueryHandler : IRequestHandler<InventoryMovementReportQuery, Result<InventoryMovementReportDto>>
{
    private readonly IApplicationDbContext _context;

    public InventoryMovementReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InventoryMovementReportDto>> Handle(InventoryMovementReportQuery request, CancellationToken cancellationToken)
    {
        // Verify store exists
        var storeExists = await _context.Stores
            .AnyAsync(s => s.Id == request.StoreId, cancellationToken);

        if (!storeExists)
        {
            return Result<InventoryMovementReportDto>.Failure("Store not found");
        }

        // Build product query
        var productQuery = _context.Products.AsQueryable();
        if (request.ProductId.HasValue)
        {
            productQuery = productQuery.Where(p => p.Id == request.ProductId.Value);
        }

        var products = await productQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var details = new List<InventoryMovementDetail>();

        foreach (var product in products)
        {
            // Get inventory levels for opening stock
            var openingStock = await _context.InventoryLevels
                .Where(il => il.StoreId == request.StoreId
                    && il.ProductVariant.ProductId == product.Id
                    && il.UpdatedAt < request.DateFrom)
                .SumAsync(il => il.OnHandQuantity, cancellationToken);

            // Get receipts
            var receipts = await _context.InventoryReceiptItems
                .Include(iri => iri.Receipt)
                .Where(iri => iri.Receipt.StoreId == request.StoreId
                    && iri.ProductVariant.ProductId == product.Id
                    && iri.Receipt.ReceiptDate >= request.DateFrom
                    && iri.Receipt.ReceiptDate <= request.DateTo
                    && iri.Receipt.Status == ReceiptStatus.Completed)
                .SumAsync(iri => iri.Quantity, cancellationToken);

            // Get issues
            var issues = await _context.InventoryIssueItems
                .Include(iii => iii.Issue)
                .Where(iii => iii.Issue.StoreId == request.StoreId
                    && iii.ProductVariant.ProductId == product.Id
                    && iii.Issue.IssueDate >= request.DateFrom
                    && iii.Issue.IssueDate <= request.DateTo
                    && iii.Issue.Status == IssueStatus.Completed)
                .SumAsync(iii => iii.Quantity, cancellationToken);

            // Get sales
            var sales = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.StoreId == request.StoreId
                    && oi.ProductVariant.ProductId == product.Id
                    && oi.Order.CreatedAt >= request.DateFrom
                    && oi.Order.CreatedAt <= request.DateTo
                    && oi.Order.Status == OrderStatus.Completed)
                .SumAsync(oi => oi.Quantity, cancellationToken);

            var closingStock = openingStock + receipts - issues - sales;

            details.Add(new InventoryMovementDetail(
                product.Id,
                product.Name,
                product.SKU,
                openingStock,
                receipts,
                issues,
                sales,
                closingStock
            ));
        }

        var report = new InventoryMovementReportDto(
            request.StoreId,
            request.ProductId,
            request.DateFrom,
            request.DateTo,
            details.Sum(d => d.OpeningStock),
            details.Sum(d => d.Receipts),
            details.Sum(d => d.Issues),
            details.Sum(d => d.Sales),
            details.Sum(d => d.ClosingStock),
            details
        );

        return Result<InventoryMovementReportDto>.Success(report);
    }
}
