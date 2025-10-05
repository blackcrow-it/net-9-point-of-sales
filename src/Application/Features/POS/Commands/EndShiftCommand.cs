using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record EndShiftCommand(
    Guid ShiftId,
    decimal ClosingCash,
    string? Notes = null
) : IRequest<Result<ShiftSummaryDto>>;

public record ShiftSummaryDto(
    Guid Id,
    Guid UserId,
    string UserName,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal OpeningCash,
    decimal ClosingCash,
    decimal ExpectedCash,
    decimal CashDifference,
    int TotalOrders,
    decimal TotalSales
);
