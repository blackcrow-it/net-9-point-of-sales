using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reports.Queries;

public class FinancialReportQueryHandler : IRequestHandler<FinancialReportQuery, Result<FinancialReportDto>>
{
    private readonly IApplicationDbContext _context;

    public FinancialReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FinancialReportDto>> Handle(FinancialReportQuery request, CancellationToken cancellationToken)
    {
        // Verify store exists
        var storeExists = await _context.Stores
            .AnyAsync(s => s.Id == request.StoreId, cancellationToken);

        if (!storeExists)
        {
            return Result<FinancialReportDto>.Failure("Store not found");
        }

        // Get all payments within date range
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.PaymentMethod)
            .Where(p => p.Order.StoreId == request.StoreId
                && p.ProcessedAt.HasValue
                && p.ProcessedAt.Value >= request.DateFrom
                && p.ProcessedAt.Value <= request.DateTo)
            .ToListAsync(cancellationToken);

        // Calculate totals by payment method
        var totalCash = payments
            .Where(p => p.PaymentMethod.Type == PaymentMethodType.Cash && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        var totalCard = payments
            .Where(p => p.PaymentMethod.Type == PaymentMethodType.Card && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        var totalEWallet = payments
            .Where(p => p.PaymentMethod.Type == PaymentMethodType.EWallet && p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        var totalRefunds = payments
            .Where(p => p.Status == PaymentStatus.Refunded)
            .Sum(p => p.Amount);

        var totalRevenue = totalCash + totalCard + totalEWallet;
        var netRevenue = totalRevenue - totalRefunds;

        // Payment breakdown
        var paymentBreakdown = payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .GroupBy(p => p.PaymentMethod.Name)
            .Select(g => new PaymentMethodBreakdown(
                g.Key,
                g.Sum(p => p.Amount),
                g.Count()
            ))
            .ToList();

        var report = new FinancialReportDto(
            request.StoreId,
            request.DateFrom,
            request.DateTo,
            totalRevenue,
            totalCash,
            totalCard,
            totalEWallet,
            totalRefunds,
            netRevenue,
            paymentBreakdown
        );

        return Result<FinancialReportDto>.Success(report);
    }
}
