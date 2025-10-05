using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

public record CompleteInventoryReceiptCommand(
    Guid ReceiptId
) : IRequest<Result<Guid>>;
