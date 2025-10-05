using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Queries;

public record GetHeldOrdersQuery(Guid StoreId) : IRequest<Result<List<OrderSummaryDto>>>;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string? CustomerName,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    string Status
);
