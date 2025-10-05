using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public class CreateShippingOrderCommandHandler : IRequestHandler<CreateShippingOrderCommand, Result<ShippingOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateShippingOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ShippingOrderDto>> Handle(CreateShippingOrderCommand request, CancellationToken cancellationToken)
    {
        // Verify order exists
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<ShippingOrderDto>.Failure("Order not found");
        }

        // Validate provider
        if (request.Provider != "GHN" && request.Provider != "GHTK")
        {
            return Result<ShippingOrderDto>.Failure("Invalid shipping provider. Must be 'GHN' or 'GHTK'");
        }

        // TODO: Integrate with actual GHN/GHTK API
        // For now, return a mock response
        var trackingNumber = $"{request.Provider}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20);
        var baseFee = request.Provider == "GHN" ? 30000m : 25000m;
        var weightFee = request.Weight * (request.Provider == "GHN" ? 5000m : 4500m);
        var shippingFee = baseFee + weightFee;
        var estimatedDays = request.Provider == "GHN" ? 3 : 4;

        var shippingOrder = new ShippingOrderDto(
            trackingNumber,
            request.Provider,
            "Pending",
            shippingFee,
            estimatedDays,
            DateTime.UtcNow
        );

        // Store tracking number in order
        // This would typically update the Order entity with tracking info
        // For now, just return the response

        return Result<ShippingOrderDto>.Success(shippingOrder);
    }
}
