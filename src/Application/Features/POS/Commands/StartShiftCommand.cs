using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record StartShiftCommand(
    Guid UserId,
    Guid StoreId,
    decimal OpeningCash
) : IRequest<Result<Guid>>;
