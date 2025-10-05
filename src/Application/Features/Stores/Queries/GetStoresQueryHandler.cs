using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stores.Queries;

public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, Result<List<StoreDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStoresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StoreDto>>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Stores.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var stores = await query
            .OrderBy(s => s.Name)
            .Select(s => new StoreDto(
                s.Id,
                s.Code,
                s.Name,
                s.Type,
                s.Address,
                s.Phone,
                s.Email,
                s.IsActive,
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<StoreDto>>.Success(stores);
    }
}
