using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class GetCustomerGroupsQueryHandler : IRequestHandler<GetCustomerGroupsQuery, Result<List<CustomerGroupDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerGroupsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CustomerGroupDto>>> Handle(GetCustomerGroupsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CustomerGroups
            .AsNoTracking()
            .AsQueryable();

        // Apply active filter if specified
        if (request.IsActive.HasValue)
        {
            query = query.Where(cg => cg.IsActive == request.IsActive.Value);
        }

        var customerGroups = await query
            .OrderBy(cg => cg.Name)
            .Select(cg => new CustomerGroupDto(
                cg.Id,
                cg.Name,
                cg.Description,
                cg.DiscountPercentage,
                cg.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<CustomerGroupDto>>.Success(customerGroups);
    }
}
