using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class GetLoyaltyPointsHistoryQueryHandler : IRequestHandler<GetLoyaltyPointsHistoryQuery, Result<PaginatedList<LoyaltyTransactionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetLoyaltyPointsHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<LoyaltyTransactionDto>>> Handle(GetLoyaltyPointsHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LoyaltyTransactions
            .AsNoTracking()
            .Include(lt => lt.Order)
            .Where(lt => lt.CustomerId == request.CustomerId)
            .OrderByDescending(lt => lt.TransactionDate);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var transactions = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(lt => new LoyaltyTransactionDto(
                lt.Id,
                lt.CustomerId,
                lt.Points,
                lt.Type.ToString(),
                lt.Description ?? string.Empty,
                lt.TransactionDate,
                lt.OrderId,
                lt.Order != null ? lt.Order.OrderNumber : null
            ))
            .ToListAsync(cancellationToken);

        var paginatedList = PaginatedList<LoyaltyTransactionDto>.Create(
            transactions,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<LoyaltyTransactionDto>>.Success(paginatedList);
    }
}
