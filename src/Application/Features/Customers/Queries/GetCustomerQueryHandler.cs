using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Queries;

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.CustomerGroup)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer == null)
            return Result<CustomerDto>.Failure("Customer not found");

        var customerDto = new CustomerDto(
            customer.Id,
            customer.CustomerNumber,
            customer.Name,
            customer.Phone,
            customer.Email,
            customer.CustomerGroupId,
            customer.CustomerGroup?.Name,
            customer.Address,
            customer.LoyaltyPoints,
            customer.TotalSpent,
            customer.TotalOrders,
            customer.LastPurchaseDate,
            customer.IsActive
        );

        return Result<CustomerDto>.Success(customerDto);
    }
}
