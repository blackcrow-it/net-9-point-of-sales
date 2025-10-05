using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class ProcessReturnCommandHandler : IRequestHandler<ProcessReturnCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public ProcessReturnCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
    {
        // Validate original order exists and is completed
        var originalOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
                .ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(o => o.Id == request.OriginalOrderId, cancellationToken);

        if (originalOrder == null)
            return Result<Guid>.Failure("Original order not found");

        if (originalOrder.Status != OrderStatus.Completed)
            return Result<Guid>.Failure("Only completed orders can be returned");

        // Validate return items
        foreach (var returnItem in request.ReturnItems)
        {
            var orderItem = originalOrder.OrderItems.FirstOrDefault(oi => oi.Id == returnItem.OrderItemId);
            if (orderItem == null)
                return Result<Guid>.Failure($"Order item {returnItem.OrderItemId} not found in original order");

            if (returnItem.Quantity > orderItem.Quantity)
                return Result<Guid>.Failure($"Return quantity ({returnItem.Quantity}) exceeds ordered quantity ({orderItem.Quantity}) for {orderItem.ProductName}");
        }

        // Generate return order number: RET-YYYYMMDD-XXXX
        var today = DateTime.UtcNow.Date;
        var todayReturnsCount = await _context.Orders
            .CountAsync(o => o.Type == OrderType.Return && o.CreatedAt >= today && o.CreatedAt < today.AddDays(1), cancellationToken);

        var returnOrderNumber = $"RET-{DateTime.UtcNow:yyyyMMdd}-{(todayReturnsCount + 1):D4}";

        // Create return order
        var returnOrder = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = returnOrderNumber,
            StoreId = request.StoreId,
            CustomerId = originalOrder.CustomerId,
            CashierId = request.ProcessedByUserId,
            ShiftId = originalOrder.ShiftId,
            Status = OrderStatus.Completed, // Return is immediately completed
            Type = OrderType.Return,
            Subtotal = 0,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = 0,
            Notes = $"Return for order {originalOrder.OrderNumber}. Reason: {request.Reason}. {request.Notes}",
            CompletedAt = DateTime.UtcNow
        };

        // Create return order items and restore inventory
        int lineNumber = 1;
        foreach (var returnItem in request.ReturnItems)
        {
            var originalItem = originalOrder.OrderItems.First(oi => oi.Id == returnItem.OrderItemId);

            var returnOrderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = returnOrder.Id,
                ProductVariantId = originalItem.ProductVariantId,
                LineNumber = lineNumber++,
                ProductSKU = originalItem.ProductSKU,
                ProductName = originalItem.ProductName,
                VariantName = originalItem.VariantName,
                Quantity = -returnItem.Quantity, // Negative quantity for returns
                Unit = originalItem.Unit,
                UnitPrice = originalItem.UnitPrice,
                DiscountAmount = 0,
                TaxRate = originalItem.TaxRate,
                TaxAmount = 0,
                LineTotal = 0
            };

            returnOrderItem.CalculateLineTotal();
            returnOrder.OrderItems.Add(returnOrderItem);

            // Restore inventory
            var inventoryLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(il => il.ProductVariantId == originalItem.ProductVariantId && il.StoreId == request.StoreId, cancellationToken);

            if (inventoryLevel != null)
            {
                inventoryLevel.AvailableQuantity += returnItem.Quantity;
                inventoryLevel.OnHandQuantity = inventoryLevel.AvailableQuantity + inventoryLevel.ReservedQuantity;
            }
        }

        // Calculate return totals (negative amounts)
        returnOrder.Subtotal = returnOrder.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
        returnOrder.TaxAmount = returnOrder.OrderItems.Sum(i => i.TaxAmount);
        returnOrder.CalculateTotalAmount();

        // Create refund payment record
        var refundAmount = Math.Abs(returnOrder.TotalAmount);
        if (refundAmount > 0)
        {
            // Use the same payment method as original order (prefer first payment method)
            var originalPaymentMethod = originalOrder.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .OrderBy(p => p.CreatedAt)
                .Select(p => p.PaymentMethodId)
                .FirstOrDefault();

            if (originalPaymentMethod != Guid.Empty)
            {
                var today2 = DateTime.UtcNow.Date;
                var todayPaymentsCount = await _context.Payments
                    .CountAsync(p => p.CreatedAt >= today2 && p.CreatedAt < today2.AddDays(1), cancellationToken);

                var refundPaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{(todayPaymentsCount + 1):D4}";

                var refundPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = returnOrder.Id,
                    PaymentMethodId = originalPaymentMethod,
                    PaymentNumber = refundPaymentNumber,
                    Amount = -refundAmount, // Negative for refund
                    Status = PaymentStatus.Completed,
                    ReferenceNumber = $"REFUND-{originalOrder.OrderNumber}",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = request.ProcessedByUserId.ToString(),
                    Notes = $"Refund for {originalOrder.OrderNumber}"
                };

                _context.Payments.Add(refundPayment);
            }
        }

        // Update original order status to Returned
        originalOrder.Status = OrderStatus.Returned;

        _context.Orders.Add(returnOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(returnOrder.Id);
    }
}
