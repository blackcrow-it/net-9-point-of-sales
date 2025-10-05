using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate shift exists and is open
        if (request.ShiftId.HasValue)
        {
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == request.ShiftId.Value, cancellationToken);

            if (shift == null)
                return Result<Guid>.Failure("Shift not found");

            if (shift.Status != ShiftStatus.Open)
                return Result<Guid>.Failure("Shift is not open");
        }

        // Generate order number: ORD-YYYYMMDD-XXXX
        var today = DateTime.UtcNow.Date;
        var todayOrdersCount = await _context.Orders
            .CountAsync(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1), cancellationToken);

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{(todayOrdersCount + 1):D4}";

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            StoreId = request.StoreId,
            CustomerId = request.CustomerId,
            CashierId = request.CreatedByUserId,
            ShiftId = request.ShiftId ?? Guid.Empty,
            Status = OrderStatus.Draft,
            Type = OrderType.Sale,
            Subtotal = 0,
            TaxAmount = 0,
            DiscountAmount = 0,
            TotalAmount = 0,
            Notes = request.Notes
        };

        // Create order items
        int lineNumber = 1;
        foreach (var itemDto in request.Items)
        {
            var productVariant = await _context.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == (itemDto.ProductVariantId ?? itemDto.ProductId), cancellationToken);

            if (productVariant == null)
                return Result<Guid>.Failure($"Product variant not found for item {lineNumber}");

            // Check inventory availability
            var inventoryLevel = await _context.InventoryLevels
                .FirstOrDefaultAsync(il => il.ProductVariantId == productVariant.Id && il.StoreId == request.StoreId, cancellationToken);

            if (inventoryLevel == null || inventoryLevel.AvailableQuantity < itemDto.Quantity)
                return Result<Guid>.Failure($"Insufficient inventory for {productVariant.Product.Name}");

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductVariantId = productVariant.Id,
                LineNumber = lineNumber++,
                ProductSKU = productVariant.SKU,
                ProductName = productVariant.Product.Name,
                VariantName = productVariant.Name,
                Quantity = itemDto.Quantity,
                Unit = "pcs", // Could be enhanced
                UnitPrice = itemDto.UnitPrice,
                DiscountAmount = itemDto.DiscountAmount,
                TaxRate = 10, // Default 10% VAT for Vietnam
                TaxAmount = 0,
                LineTotal = 0
            };

            orderItem.CalculateLineTotal();
            orderItem.Validate();

            order.OrderItems.Add(orderItem);

            // Reserve inventory
            inventoryLevel.AvailableQuantity -= itemDto.Quantity;
            inventoryLevel.ReservedQuantity += itemDto.Quantity;
        }

        // Calculate order totals
        order.Subtotal = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice - i.DiscountAmount);
        order.TaxAmount = order.OrderItems.Sum(i => i.TaxAmount);
        order.DiscountAmount = order.OrderItems.Sum(i => i.DiscountAmount);
        order.CalculateTotalAmount();

        order.Validate();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}
