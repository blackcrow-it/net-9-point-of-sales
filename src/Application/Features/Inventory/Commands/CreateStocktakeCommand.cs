using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

public record CreateStocktakeCommand(
    Guid StoreId,
    string Reference,
    DateTime ScheduledDate,
    List<Guid> ProductIds,
    string? Notes = null
) : IRequest<Result<Guid>>;
