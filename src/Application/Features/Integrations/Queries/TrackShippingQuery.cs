using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Queries;

public record TrackShippingQuery(
    string TrackingNumber,
    string Provider // "GHN" or "GHTK"
) : IRequest<Result<ShippingStatusDto>>;

public record ShippingStatusDto(
    string TrackingNumber,
    string Provider,
    string Status,
    string CurrentLocation,
    DateTime? EstimatedDelivery,
    List<ShippingEvent> Events
);

public record ShippingEvent(
    DateTime Timestamp,
    string Status,
    string Location,
    string Description
);
