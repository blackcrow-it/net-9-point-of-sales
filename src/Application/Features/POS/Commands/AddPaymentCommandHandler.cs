using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        // Validate order exists
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Result<Guid>.Failure("Order not found");

        // Validate payment method exists
        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, cancellationToken);

        if (paymentMethod == null)
            return Result<Guid>.Failure("Payment method not found");

        if (!paymentMethod.IsActive)
            return Result<Guid>.Failure("Payment method is not active");

        // Validate total payments <= order total amount
        var totalPaid = order.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        if (totalPaid + request.Amount > order.TotalAmount)
            return Result<Guid>.Failure($"Payment amount exceeds order total. Remaining: {order.TotalAmount - totalPaid:N2}");

        // Validate reference number if required
        if (paymentMethod.RequiresReference && string.IsNullOrWhiteSpace(request.TransactionId))
            return Result<Guid>.Failure($"Transaction ID is required for {paymentMethod.Name}");

        // Generate payment number: PAY-YYYYMMDD-XXXX
        var today = DateTime.UtcNow.Date;
        var todayPaymentsCount = await _context.Payments
            .CountAsync(p => p.CreatedAt >= today && p.CreatedAt < today.AddDays(1), cancellationToken);

        var paymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{(todayPaymentsCount + 1):D4}";

        // Create payment
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            PaymentMethodId = request.PaymentMethodId,
            PaymentNumber = paymentNumber,
            Amount = request.Amount,
            Status = PaymentStatus.Pending,
            ReferenceNumber = request.TransactionId,
            Notes = request.Notes
        };

        payment.Validate();

        // Mark as completed for cash payments, pending for others
        if (paymentMethod.Type == PaymentMethodType.Cash)
        {
            payment.MarkAsCompleted();
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(payment.Id);
    }
}
