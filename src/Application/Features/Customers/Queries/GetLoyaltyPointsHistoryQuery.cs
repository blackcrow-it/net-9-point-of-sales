using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve loyalty points history for a customer
/// </summary>
public record GetLoyaltyPointsHistoryQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<LoyaltyTransactionDto>>>;

public record LoyaltyTransactionDto(
    Guid Id,
    Guid CustomerId,
    decimal Points,
    string Type,
    string Description,
    DateTime TransactionDate,
    Guid? OrderId,
    string? OrderNumber
);
