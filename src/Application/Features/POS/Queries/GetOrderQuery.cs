using Application.Common.Models;
using Domain.Entities.Sales;
using MediatR;

namespace Application.Features.POS.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<Result<OrderDetailDto>>;

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid StoreId,
    Guid? CustomerId,
    string? CustomerName,
    Guid? ShiftId,
    OrderStatus Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal FinalAmount,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<OrderItemDetailDto> Items,
    List<PaymentDetailDto> Payments,
    string? Notes
);

public record OrderItemDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? ProductVariantId,
    string? VariantName,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal LineTotal
);

public record PaymentDetailDto(
    Guid Id,
    Guid PaymentMethodId,
    string PaymentMethodName,
    decimal Amount,
    string Status,
    DateTime? CompletedAt,
    string? TransactionId
);
