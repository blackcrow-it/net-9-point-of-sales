using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class StartShiftCommandHandler : IRequestHandler<StartShiftCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public StartShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(StartShiftCommand request, CancellationToken cancellationToken)
    {
        // Validate: Only one open shift per cashier
        var existingOpenShift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.CashierId == request.UserId && s.Status == ShiftStatus.Open, cancellationToken);

        if (existingOpenShift != null)
            return Result<Guid>.Failure("Cashier already has an open shift. Please close the existing shift first.");

        // Generate shift number: SFT-YYYYMMDD-XXXX
        var today = DateTime.UtcNow.Date;
        var todayShiftsCount = await _context.Shifts
            .CountAsync(s => s.CreatedAt >= today && s.CreatedAt < today.AddDays(1), cancellationToken);

        var shiftNumber = $"SFT-{DateTime.UtcNow:yyyyMMdd}-{(todayShiftsCount + 1):D4}";

        // Create new shift
        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            StoreId = request.StoreId,
            CashierId = request.UserId,
            ShiftNumber = shiftNumber,
            Status = ShiftStatus.Open,
            StartTime = DateTime.UtcNow,
            OpeningCash = request.OpeningCash,
            ClosingCash = 0,
            ExpectedCash = 0,
            CashDifference = 0,
            TotalSales = 0,
            TotalTransactions = 0
        };

        shift.Validate();

        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(shift.Id);
    }
}
