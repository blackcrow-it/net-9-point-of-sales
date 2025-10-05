using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Queries;

/// <summary>
/// Query to search customers by phone, name, or email
/// </summary>
public record SearchCustomersQuery(
    string SearchTerm
) : IRequest<Result<List<CustomerDto>>>;
