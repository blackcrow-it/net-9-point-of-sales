using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record CompleteOrderCommand(
    Guid OrderId,
    Guid ProcessedByUserId
) : IRequest<Result<OrderReceiptDto>>;

public record OrderReceiptDto(
    Guid OrderId,
    string OrderNumber,
    string StoreName,
    string? CustomerName,
    DateTime CompletedAt,
    List<OrderItemDetailDto> Items,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal FinalAmount,
    List<PaymentDetailDto> Payments
);

public record OrderItemDetailDto(
    string ProductName,
    string? VariantName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record PaymentDetailDto(
    string PaymentMethod,
    decimal Amount
);
