using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

public record GetCustomersQuery(
    Guid? CustomerGroupId,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<CustomerDto>>>;
