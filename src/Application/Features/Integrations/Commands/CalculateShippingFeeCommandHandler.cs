using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Commands;

public class CalculateShippingFeeCommandHandler : IRequestHandler<CalculateShippingFeeCommand, Result<ShippingFeeResponseDto>>
{
    public async Task<Result<ShippingFeeResponseDto>> Handle(CalculateShippingFeeCommand request, CancellationToken cancellationToken)
    {
        // Validate provider
        if (request.Provider != "GHN" && request.Provider != "GHTK")
        {
            return Result<ShippingFeeResponseDto>.Failure("Invalid shipping provider. Must be 'GHN' or 'GHTK'");
        }

        // TODO: Integrate with actual GHN/GHTK API
        // For now, return a mock response based on weight
        var baseFee = request.Provider == "GHN" ? 30000m : 25000m;
        var weightFee = request.Weight * (request.Provider == "GHN" ? 5000m : 4500m);
        var totalFee = baseFee + weightFee;
        
        var estimatedDays = request.Provider == "GHN" ? 3 : 4;
        var serviceType = request.Provider == "GHN" ? "Express" : "Standard";

        var response = new ShippingFeeResponseDto(
            request.Provider,
            totalFee,
            estimatedDays,
            serviceType
        );

        return await Task.FromResult(Result<ShippingFeeResponseDto>.Success(response));
    }
}
