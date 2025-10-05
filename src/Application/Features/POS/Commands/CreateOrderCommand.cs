using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record CreateOrderCommand(
    Guid StoreId,
    Guid? CustomerId,
    Guid? ShiftId,
    Guid CreatedByUserId,
    List<OrderItemDto> Items,
    string? Notes = null
) : IRequest<Result<Guid>>;

public record OrderItemDto(
    Guid ProductId,
    Guid? ProductVariantId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount = 0
);
