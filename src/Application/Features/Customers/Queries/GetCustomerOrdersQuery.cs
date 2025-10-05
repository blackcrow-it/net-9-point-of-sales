using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve customer purchase history
/// </summary>
public record GetCustomerOrdersQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<OrderSummaryDto>>>;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    DateTime CreatedAt,
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal TotalAmount,
    string Status,
    int ItemCount
);
