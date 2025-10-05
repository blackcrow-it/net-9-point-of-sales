using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Inventory.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .AsQueryable();

        // Apply filters
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.SKU,
                p.Name,
                p.Description,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.BrandId,
                p.Brand != null ? p.Brand.Name : null,
                p.CostPrice ?? 0,
                p.RetailPrice ?? 0,
                p.Unit ?? "pcs",
                p.TrackInventory,
                p.ReorderLevel,
                p.ReorderQuantity,
                p.ImageUrl,
                p.IsActive,
                p.Variants.Count,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<ProductDto>.Create(
            products,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<ProductDto>>.Success(paginatedList);
    }
}
