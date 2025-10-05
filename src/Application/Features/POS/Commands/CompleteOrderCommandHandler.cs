using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Customers;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<OrderReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public CompleteOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderReceiptDto>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        // Load order with all related entities
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
                .ThenInclude(p => p.PaymentMethod)
            .Include(o => o.Customer)
            .Include(o => o.Store)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Result<OrderReceiptDto>.Failure("Order not found");

        // Validate payments
        var totalPaid = order.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        if (totalPaid < order.TotalAmount)
            return Result<OrderReceiptDto>.Failure($"Insufficient payment. Required: {order.TotalAmount:N2}, Paid: {totalPaid:N2}");

        // Complete the order using domain method
        try
        {
            order.Complete();
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderReceiptDto>.Failure(ex.Message);
        }

        // Commit inventory - convert reserved quantity to sold
        foreach (var item in order.OrderItems)
        {
            var inventoryLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(il => il.ProductVariantId == item.ProductVariantId && il.StoreId == order.StoreId, cancellationToken);

            if (inventoryLevel != null)
            {
                // Reserved quantity was already decremented during order creation
                // Now we just need to reduce reserved quantity
                inventoryLevel.ReservedQuantity -= item.Quantity;
                inventoryLevel.OnHandQuantity = inventoryLevel.AvailableQuantity + inventoryLevel.ReservedQuantity;
            }
        }

        // Create loyalty transaction if customer exists
        if (order.CustomerId.HasValue)
        {
            var customer = await _context.Customers
                .Include(c => c.CustomerGroup)
                .FirstOrDefaultAsync(c => c.Id == order.CustomerId, cancellationToken);

            if (customer != null)
            {
                // Calculate loyalty points (1% of order total, can be multiplied by group multiplier)
                decimal pointsMultiplier = customer.CustomerGroup?.LoyaltyPointsMultiplier ?? 1.0m;
                decimal pointsEarned = Math.Floor((order.TotalAmount * 0.01m) * pointsMultiplier);

                if (pointsEarned > 0)
                {
                    customer.AddLoyaltyPoints(pointsEarned);

                    var loyaltyTransaction = new LoyaltyTransaction
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        OrderId = order.Id,
                        Type = LoyaltyTransactionType.Earned,
                        Points = pointsEarned,
                        TransactionDate = DateTime.UtcNow,
                        Description = $"Points earned from order {order.OrderNumber}"
                    };

                    _context.LoyaltyTransactions.Add(loyaltyTransaction);
                }

                // Update customer purchase statistics
                customer.RecordPurchase(order.TotalAmount);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Publish OrderCompletedEvent for SignalR notification
        // This would be implemented when domain events infrastructure is set up

        // Build receipt DTO
        var receiptDto = new OrderReceiptDto(
            OrderId: order.Id,
            OrderNumber: order.OrderNumber,
            StoreName: order.Store.Name,
            CustomerName: order.Customer?.Name,
            CompletedAt: order.CompletedAt!.Value,
            Items: order.OrderItems.Select(i => new OrderItemDetailDto(
                ProductName: i.ProductName,
                VariantName: i.VariantName,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice,
                LineTotal: i.LineTotal
            )).ToList(),
            TotalAmount: order.Subtotal,
            DiscountAmount: order.DiscountAmount,
            TaxAmount: order.TaxAmount,
            FinalAmount: order.TotalAmount,
            Payments: order.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .Select(p => new PaymentDetailDto(
                    PaymentMethod: p.PaymentMethod.Name,
                    Amount: p.Amount
                )).ToList()
        );

        return Result<OrderReceiptDto>.Success(receiptDto);
    }
}
