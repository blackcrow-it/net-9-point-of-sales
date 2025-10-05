using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

/// <summary>
/// Command to finalize stocktake and adjust inventory based on counted quantities
/// </summary>
public record FinalizeStocktakeCommand(
    Guid StocktakeId
) : IRequest<Result<StocktakeSummaryDto>>;

public record StocktakeSummaryDto(
    Guid StocktakeId,
    string StocktakeNumber,
    int TotalItems,
    int ItemsWithVariance,
    decimal TotalVarianceValue,
    List<VarianceItemDto> VarianceItems
);

public record VarianceItemDto(
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    int SystemQuantity,
    int CountedQuantity,
    int Variance,
    decimal UnitCost,
    decimal VarianceValue
);
