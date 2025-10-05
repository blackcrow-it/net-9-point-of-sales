using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Queries;

public class GenerateReceiptQueryHandler : IRequestHandler<GenerateReceiptQuery, Result<ReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public GenerateReceiptQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReceiptDto>> Handle(GenerateReceiptQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Store)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
                .ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Result<ReceiptDto>.Failure("Order not found");

        if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Returned)
            return Result<ReceiptDto>.Failure("Receipt can only be generated for completed or returned orders");

        // Calculate loyalty points earned (if customer exists)
        decimal loyaltyPointsEarned = 0;
        if (order.CustomerId.HasValue && order.Type == OrderType.Sale)
        {
            var loyaltyTransaction = await _context.LoyaltyTransactions
                .AsNoTracking()
                .Where(lt => lt.OrderId == order.Id && lt.Type == Domain.Entities.Customers.LoyaltyTransactionType.Earned)
                .FirstOrDefaultAsync(cancellationToken);

            loyaltyPointsEarned = loyaltyTransaction?.Points ?? 0;
        }

        var receiptDto = new ReceiptDto(
            OrderId: order.Id,
            OrderNumber: order.OrderNumber,
            OrderType: order.Type.ToString(),
            CompletedAt: order.CompletedAt ?? DateTime.UtcNow,
            Store: new StoreInfoDto(
                Name: order.Store.Name,
                Address: order.Store.Address,
                Phone: order.Store.Phone,
                TaxCode: null // TaxCode not yet implemented in Store entity
            ),
            Customer: order.Customer != null ? new CustomerInfoDto(
                Name: order.Customer.Name,
                Phone: order.Customer.Phone,
                LoyaltyPointsEarned: loyaltyPointsEarned,
                TotalLoyaltyPoints: order.Customer.LoyaltyPoints
            ) : null,
            Items: order.OrderItems
                .OrderBy(i => i.LineNumber)
                .Select(i => new ReceiptItemDto(
                    LineNumber: i.LineNumber,
                    ProductName: i.ProductName,
                    VariantName: i.VariantName,
                    Quantity: i.Quantity,
                    Unit: i.Unit,
                    UnitPrice: i.UnitPrice,
                    DiscountAmount: i.DiscountAmount,
                    LineTotal: i.LineTotal
                )).ToList(),
            Totals: new ReceiptTotalsDto(
                Subtotal: order.Subtotal,
                DiscountAmount: order.DiscountAmount,
                TaxAmount: order.TaxAmount,
                TotalAmount: order.TotalAmount
            ),
            Payments: order.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Select(p => new ReceiptPaymentDto(
                    PaymentMethod: p.PaymentMethod.Name,
                    Amount: p.Amount,
                    ReferenceNumber: p.ReferenceNumber
                )).ToList(),
            Notes: order.Notes
        );

        return Result<ReceiptDto>.Success(receiptDto);
    }
}
