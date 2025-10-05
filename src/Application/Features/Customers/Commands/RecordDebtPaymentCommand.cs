using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Commands;

/// <summary>
/// Command to record payment towards debt
/// </summary>
public record RecordDebtPaymentCommand(
    Guid DebtId,
    decimal PaymentAmount,
    string? Notes = null
) : IRequest<Result<Guid>>;
