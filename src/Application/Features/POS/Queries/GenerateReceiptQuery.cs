using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Queries;

public record GenerateReceiptQuery(Guid OrderId) : IRequest<Result<ReceiptDto>>;

public record ReceiptDto(
    Guid OrderId,
    string OrderNumber,
    string OrderType,
    DateTime CompletedAt,
    StoreInfoDto Store,
    CustomerInfoDto? Customer,
    List<ReceiptItemDto> Items,
    ReceiptTotalsDto Totals,
    List<ReceiptPaymentDto> Payments,
    string? Notes
);

public record StoreInfoDto(
    string Name,
    string? Address,
    string? Phone,
    string? TaxCode
);

public record CustomerInfoDto(
    string Name,
    string? Phone,
    decimal LoyaltyPointsEarned,
    decimal TotalLoyaltyPoints
);

public record ReceiptItemDto(
    int LineNumber,
    string ProductName,
    string? VariantName,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal LineTotal
);

public record ReceiptTotalsDto(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount
);

public record ReceiptPaymentDto(
    string PaymentMethod,
    decimal Amount,
    string? ReferenceNumber
);
