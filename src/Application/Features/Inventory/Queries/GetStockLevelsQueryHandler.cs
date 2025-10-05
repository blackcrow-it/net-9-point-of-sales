using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Queries;

public class GetStockLevelsQueryHandler : IRequestHandler<GetStockLevelsQuery, Result<PaginatedList<StockLevelDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStockLevelsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<StockLevelDto>>> Handle(GetStockLevelsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryLevels
            .AsNoTracking()
            .Include(il => il.Store)
            .Include(il => il.ProductVariant)
                .ThenInclude(pv => pv!.Product)
            .Where(il => il.StoreId == request.StoreId)
            .AsQueryable();

        // Filter by product if specified
        if (request.ProductId.HasValue)
        {
            query = query.Where(il => il.ProductVariant!.ProductId == request.ProductId.Value);
        }

        // Filter low stock items (using a threshold of 10 as default)
        if (request.LowStock.HasValue && request.LowStock.Value)
        {
            query = query.Where(il => il.AvailableQuantity <= 10);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var stockLevels = await query
            .OrderBy(il => il.ProductVariant!.Product!.Name)
            .ThenBy(il => il.ProductVariant!.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(il => new StockLevelDto(
                il.Id,
                il.StoreId,
                il.Store!.Name,
                il.ProductVariant!.ProductId,
                il.ProductVariant.Product!.Name,
                il.ProductVariant.Product.SKU,
                il.ProductVariantId,
                il.ProductVariant.Name,
                il.OnHandQuantity,
                il.AvailableQuantity,
                il.ReservedQuantity,
                il.LastCountedAt
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<StockLevelDto>.Create(
            stockLevels,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<StockLevelDto>>.Success(paginatedList);
    }
}
