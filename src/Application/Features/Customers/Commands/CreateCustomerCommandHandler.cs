using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Validate unique constraints
        if (!string.IsNullOrEmpty(request.Phone))
        {
            var phoneExists = await _context.Customers
                .AnyAsync(c => c.Phone == request.Phone, cancellationToken);
            if (phoneExists)
                return Result<Guid>.Failure("Phone number already exists");
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == request.Email, cancellationToken);
            if (emailExists)
                return Result<Guid>.Failure("Email already exists");
        }

        // Generate customer number
        var today = DateTime.UtcNow.Date;
        var todayCustomersCount = await _context.Customers
            .CountAsync(c => c.CreatedAt >= today && c.CreatedAt < today.AddDays(1), cancellationToken);
        var customerNumber = $"CUS-{DateTime.UtcNow:yyyyMMdd}{(todayCustomersCount + 1):D4}";

        // Create customer
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerNumber = customerNumber,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            CustomerGroupId = request.CustomerGroupId,
            Address = request.Address,
            IsActive = request.IsActive,
            LoyaltyPoints = 0,
            TotalSpent = 0,
            TotalOrders = 0
        };

        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id);
    }
}
