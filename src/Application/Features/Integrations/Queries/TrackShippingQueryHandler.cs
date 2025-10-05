using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Queries;

public class TrackShippingQueryHandler : IRequestHandler<TrackShippingQuery, Result<ShippingStatusDto>>
{
    public async Task<Result<ShippingStatusDto>> Handle(TrackShippingQuery request, CancellationToken cancellationToken)
    {
        // Validate provider
        if (request.Provider != "GHN" && request.Provider != "GHTK")
        {
            return Result<ShippingStatusDto>.Failure("Invalid shipping provider. Must be 'GHN' or 'GHTK'");
        }

        // TODO: Integrate with actual GHN/GHTK tracking API
        // For now, return a mock response with sample tracking events
        var events = new List<ShippingEvent>
        {
            new ShippingEvent(
                DateTime.UtcNow.AddDays(-2),
                "Picked Up",
                "Hanoi Hub",
                "Package picked up from sender"
            ),
            new ShippingEvent(
                DateTime.UtcNow.AddDays(-1),
                "In Transit",
                "Ho Chi Minh Hub",
                "Package arrived at sorting facility"
            ),
            new ShippingEvent(
                DateTime.UtcNow,
                "Out for Delivery",
                "District 1 Station",
                "Package is out for delivery"
            )
        };

        var status = new ShippingStatusDto(
            request.TrackingNumber,
            request.Provider,
            "Out for Delivery",
            "District 1 Station",
            DateTime.UtcNow.AddHours(4),
            events
        );

        return await Task.FromResult(Result<ShippingStatusDto>.Success(status));
    }
}
