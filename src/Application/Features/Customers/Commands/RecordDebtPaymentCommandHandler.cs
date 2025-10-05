using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Customers.Commands;

public class RecordDebtPaymentCommandHandler : IRequestHandler<RecordDebtPaymentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RecordDebtPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RecordDebtPaymentCommand request, CancellationToken cancellationToken)
    {
        // Retrieve debt
        var debt = await _context.Debts
            .FirstOrDefaultAsync(d => d.Id == request.DebtId, cancellationToken);

        if (debt == null)
            return Result<Guid>.Failure("Debt not found");

        if (request.PaymentAmount <= 0)
            return Result<Guid>.Failure("Payment amount must be greater than zero");

        if (request.PaymentAmount > debt.RemainingAmount)
            return Result<Guid>.Failure($"Payment amount cannot exceed remaining amount ({debt.RemainingAmount})");

        // Update debt amounts
        debt.PaidAmount += request.PaymentAmount;
        debt.RemainingAmount -= request.PaymentAmount;

        // Update status
        if (debt.RemainingAmount == 0)
        {
            debt.Status = DebtStatus.Paid;
        }
        else if (debt.PaidAmount > 0)
        {
            debt.Status = DebtStatus.PartiallyPaid;
        }

        // Update notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            debt.Notes = string.IsNullOrWhiteSpace(debt.Notes)
                ? request.Notes
                : $"{debt.Notes}\n{DateTime.UtcNow:yyyy-MM-dd}: {request.Notes}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(debt.Id);
    }
}
