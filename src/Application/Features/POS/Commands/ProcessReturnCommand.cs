using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record ProcessReturnCommand(
    Guid OriginalOrderId,
    Guid StoreId,
    Guid ProcessedByUserId,
    List<ReturnItemDto> ReturnItems,
    string Reason,
    string? Notes = null
) : IRequest<Result<Guid>>;

public record ReturnItemDto(
    Guid OrderItemId,
    decimal Quantity,
    string? Reason = null
);
