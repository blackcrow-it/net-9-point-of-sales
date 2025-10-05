using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class CreateDebtCommandHandler : IRequestHandler<CreateDebtCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateDebtCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateDebtCommand request, CancellationToken cancellationToken)
    {
        // Verify customer exists
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (!customerExists)
            return Result<Guid>.Failure("Customer not found");

        // Verify order exists
        var orderExists = await _context.Orders
            .AnyAsync(o => o.Id == request.OrderId, cancellationToken);

        if (!orderExists)
            return Result<Guid>.Failure("Order not found");

        if (request.Amount <= 0)
            return Result<Guid>.Failure("Debt amount must be greater than zero");

        // Generate debt number
        var today = DateTime.UtcNow;
        var dailyCount = await _context.Debts
            .CountAsync(d => d.CreatedAt.Date == today.Date, cancellationToken);
        var debtNumber = $"DBT-{today:yyyyMMdd}-{(dailyCount + 1):D4}";

        // Create debt
        var debt = new Debt
        {
            Id = Guid.NewGuid(),
            DebtNumber = debtNumber,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaidAmount = 0,
            RemainingAmount = request.Amount,
            DueDate = request.DueDate,
            Status = DebtStatus.Pending,
            Notes = request.Notes
        };

        _context.Debts.Add(debt);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(debt.Id);
    }
}
