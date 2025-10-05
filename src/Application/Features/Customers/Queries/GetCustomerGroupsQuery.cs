using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

/// <summary>
/// Query to retrieve customer groups
/// </summary>
public record GetCustomerGroupsQuery(
    bool? IsActive = null
) : IRequest<Result<List<CustomerGroupDto>>>;

public record CustomerGroupDto(
    Guid Id,
    string Name,
    string? Description,
    decimal DiscountPercentage,
    bool IsActive
);
