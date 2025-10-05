using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class EndShiftCommandHandler : IRequestHandler<EndShiftCommand, Result<ShiftSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public EndShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ShiftSummaryDto>> Handle(EndShiftCommand request, CancellationToken cancellationToken)
    {
        // Find shift
        var shift = await _context.Shifts
            .Include(s => s.Cashier)
            .Include(s => s.Orders)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift == null)
            return Result<ShiftSummaryDto>.Failure("Shift not found");

        if (shift.Status == ShiftStatus.Closed)
            return Result<ShiftSummaryDto>.Failure("Shift is already closed");

        // Calculate shift summary
        var cashOrders = shift.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .ToList();

        var totalSales = cashOrders.Sum(o => o.TotalAmount);
        var totalOrders = cashOrders.Count;

        // Calculate expected cash (opening cash + cash payments)
        var cashPayments = await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => cashOrders.Select(o => o.Id).Contains(p.OrderId) && p.PaymentMethod.Type == PaymentMethodType.Cash)
            .SumAsync(p => p.Amount, cancellationToken);

        var expectedCash = shift.OpeningCash + cashPayments;

        // Update shift
        shift.TotalSales = totalSales;
        shift.TotalTransactions = totalOrders;
        shift.Notes = request.Notes;
        shift.CloseShift(request.ClosingCash, expectedCash);

        await _context.SaveChangesAsync(cancellationToken);

        // Build response
        var response = new ShiftSummaryDto(
            shift.Id,
            shift.CashierId,
            shift.Cashier.FullName,
            shift.StartTime,
            shift.EndTime,
            shift.OpeningCash,
            shift.ClosingCash,
            shift.ExpectedCash,
            shift.CashDifference,
            shift.TotalTransactions,
            shift.TotalSales
        );

        return Result<ShiftSummaryDto>.Success(response);
    }
}
