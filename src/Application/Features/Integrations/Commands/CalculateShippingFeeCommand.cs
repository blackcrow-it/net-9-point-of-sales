using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Commands;

public record CalculateShippingFeeCommand(
    string FromAddress,
    string ToAddress,
    decimal Weight,
    string Provider // "GHN" or "GHTK"
) : IRequest<Result<ShippingFeeResponseDto>>;

public record ShippingFeeResponseDto(
    string Provider,
    decimal Fee,
    int EstimatedDeliveryDays,
    string ServiceType
);
