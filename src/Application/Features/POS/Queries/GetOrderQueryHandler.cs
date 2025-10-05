using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.POS.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, Result<OrderDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderDetailDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
                .ThenInclude(p => p.PaymentMethod)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Result<OrderDetailDto>.Failure("Order not found");

        var orderDto = new OrderDetailDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            StoreId: order.StoreId,
            CustomerId: order.CustomerId,
            CustomerName: order.Customer?.Name,
            ShiftId: order.ShiftId,
            Status: order.Status,
            TotalAmount: order.Subtotal,
            DiscountAmount: order.DiscountAmount,
            TaxAmount: order.TaxAmount,
            FinalAmount: order.TotalAmount,
            CreatedAt: order.CreatedAt,
            CompletedAt: order.CompletedAt,
            Items: order.OrderItems.Select(i => new OrderItemDetailDto(
                Id: i.Id,
                ProductId: i.ProductVariantId,
                ProductName: i.ProductName,
                ProductVariantId: i.ProductVariantId,
                VariantName: i.VariantName,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice,
                DiscountAmount: i.DiscountAmount,
                LineTotal: i.LineTotal
            )).ToList(),
            Payments: order.Payments.Select(p => new PaymentDetailDto(
                Id: p.Id,
                PaymentMethodId: p.PaymentMethodId,
                PaymentMethodName: p.PaymentMethod.Name,
                Amount: p.Amount,
                Status: p.Status.ToString(),
                CompletedAt: p.ProcessedAt,
                TransactionId: p.ReferenceNumber
            )).ToList(),
            Notes: order.Notes
        );

        return Result<OrderDetailDto>.Success(orderDto);
    }
}
