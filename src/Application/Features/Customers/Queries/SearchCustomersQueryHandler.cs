using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, Result<List<CustomerDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CustomerDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return Result<List<CustomerDto>>.Failure("Search term is required");

        var searchTerm = request.SearchTerm.Trim().ToLower();

        var customers = await _context.Customers
            .AsNoTracking()
            .Include(c => c.CustomerGroup)
            .Where(c => 
                c.Phone != null && c.Phone.ToLower().Contains(searchTerm) ||
                c.Email != null && c.Email.ToLower().Contains(searchTerm) ||
                c.Name.ToLower().Contains(searchTerm) ||
                c.CustomerNumber.ToLower().Contains(searchTerm)
            )
            .OrderBy(c => c.Name)
            .Take(50) // Limit results for performance
            .Select(c => new CustomerDto(
                c.Id,
                c.CustomerNumber,
                c.Name,
                c.Phone,
                c.Email,
                c.CustomerGroupId,
                c.CustomerGroup != null ? c.CustomerGroup.Name : null,
                c.Address,
                c.LoyaltyPoints,
                c.TotalSpent,
                c.TotalOrders,
                c.LastPurchaseDate,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<CustomerDto>>.Success(customers);
    }
}
