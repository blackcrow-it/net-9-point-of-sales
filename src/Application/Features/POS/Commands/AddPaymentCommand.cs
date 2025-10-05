using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record AddPaymentCommand(
    Guid OrderId,
    Guid PaymentMethodId,
    decimal Amount,
    string? TransactionId = null,
    string? Notes = null
) : IRequest<Result<Guid>>;
