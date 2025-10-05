using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Queries;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PaginatedList<ProductSearchResultDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ProductSearchResultDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        // Start with all active product variants
        var query = _context.ProductVariants
            .AsNoTracking()
            .Include(pv => pv.Product)
                .ThenInclude(p => p.Category)
            .Where(pv => pv.Product.IsActive);

        // Apply search term filter (search by product name, SKU, or barcode)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(pv =>
                pv.Product.Name.ToLower().Contains(searchTerm) ||
                pv.Product.SKU.ToLower().Contains(searchTerm) ||
                pv.SKU.ToLower().Contains(searchTerm) ||
                (pv.Barcode != null && pv.Barcode.ToLower().Contains(searchTerm))
            );
        }

        // Apply category filter
        if (request.CategoryId.HasValue)
        {
            query = query.Where(pv => pv.Product.CategoryId == request.CategoryId.Value);
        }

        // Get inventory levels for the specified store
        var inventoryLevelsDict = await _context.InventoryLevels
            .AsNoTracking()
            .Where(il => il.StoreId == request.StoreId)
            .ToDictionaryAsync(il => il.ProductVariantId, il => il.AvailableQuantity, cancellationToken);

        // Order by product name, then variant name
        var orderedQuery = query.OrderBy(pv => pv.Product.Name).ThenBy(pv => pv.Name);

        // Get total count before pagination
        var totalCount = await orderedQuery.CountAsync(cancellationToken);

        // Apply pagination
        var items = await orderedQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pv => new
            {
                pv.Id,
                pv.ProductId,
                pv.SKU,
                ProductName = pv.Product.Name,
                pv.Name,
                pv.Barcode,
                RetailPrice = pv.RetailPrice ?? pv.Product.RetailPrice ?? 0,
                pv.Product.Unit,
                pv.Product.TrackInventory,
                CategoryName = pv.Product.Category != null ? pv.Product.Category.Name : null,
                ImageUrl = pv.ImageUrl ?? pv.Product.ImageUrl
            })
            .ToListAsync(cancellationToken);

        // Map to DTOs with inventory information
        var results = items.Select(item => new ProductSearchResultDto(
            Id: item.Id,
            ProductId: item.ProductId,
            SKU: item.SKU,
            Name: item.ProductName,
            VariantName: item.Name,
            Barcode: item.Barcode,
            RetailPrice: item.RetailPrice,
            AvailableQuantity: inventoryLevelsDict.TryGetValue(item.Id, out var quantity) ? quantity : 0,
            Unit: item.Unit,
            TrackInventory: item.TrackInventory,
            CategoryName: item.CategoryName,
            ImageUrl: item.ImageUrl
        )).ToList();

        var paginatedList = PaginatedList<ProductSearchResultDto>.Create(
            results,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<ProductSearchResultDto>>.Success(paginatedList);
    }
}
