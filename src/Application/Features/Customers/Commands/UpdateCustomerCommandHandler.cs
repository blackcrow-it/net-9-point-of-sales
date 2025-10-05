using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
            return Result.Failure("Customer not found");

        // Validate unique constraints
        if (!string.IsNullOrEmpty(request.Phone) && request.Phone != customer.Phone)
        {
            var phoneExists = await _context.Customers
                .AnyAsync(c => c.Phone == request.Phone && c.Id != request.Id, cancellationToken);
            if (phoneExists)
                return Result.Failure("Phone number already exists");
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Email != customer.Email)
        {
            var emailExists = await _context.Customers
                .AnyAsync(c => c.Email == request.Email && c.Id != request.Id, cancellationToken);
            if (emailExists)
                return Result.Failure("Email already exists");
        }

        // Update customer
        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.CustomerGroupId = request.CustomerGroupId;
        customer.Address = request.Address;
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
