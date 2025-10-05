using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class AddLoyaltyPointsCommandHandler : IRequestHandler<AddLoyaltyPointsCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddLoyaltyPointsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddLoyaltyPointsCommand request, CancellationToken cancellationToken)
    {
        // Retrieve customer
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer == null)
            return Result<Guid>.Failure("Customer not found");

        if (request.Points <= 0)
            return Result<Guid>.Failure("Points must be greater than zero");

        // Parse transaction type
        if (!Enum.TryParse<LoyaltyTransactionType>(request.Type, true, out var transactionType))
            return Result<Guid>.Failure("Invalid transaction type. Must be Earned, Redeemed, Adjusted, or Expired");

        // Create loyalty transaction
        var transaction = new LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Points = request.Points,
            Type = transactionType,
            Description = request.Description,
            OrderId = request.OrderId,
            TransactionDate = DateTime.UtcNow
        };

        _context.LoyaltyTransactions.Add(transaction);

        // Update customer loyalty points using domain method
        customer.AddLoyaltyPoints(request.Points);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transaction.Id);
    }
}
