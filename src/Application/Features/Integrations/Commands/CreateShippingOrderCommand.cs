using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Commands;

public record CreateShippingOrderCommand(
    Guid OrderId,
    string Provider, // "GHN" or "GHTK"
    string RecipientName,
    string RecipientPhone,
    string RecipientAddress,
    decimal Weight,
    decimal CodAmount = 0
) : IRequest<Result<ShippingOrderDto>>;

public record ShippingOrderDto(
    string TrackingNumber,
    string Provider,
    string Status,
    decimal ShippingFee,
    int EstimatedDeliveryDays,
    DateTime CreatedAt
);
