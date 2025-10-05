using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

public record GetCustomerQuery(Guid CustomerId) : IRequest<Result<CustomerDto>>;

public record CustomerDto(
    Guid Id,
    string Code,
    string Name,
    string? Phone,
    string? Email,
    Guid? CustomerGroupId,
    string? CustomerGroupName,
    string? Address,
    decimal LoyaltyPoints,
    decimal TotalPurchases,
    int TotalOrders,
    DateTime? LastPurchaseDate,
    bool IsActive
);
