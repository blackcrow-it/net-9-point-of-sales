using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Commands;

/// <summary>
/// Command to create customer debt record
/// </summary>
public record CreateDebtCommand(
    Guid CustomerId,
    Guid OrderId,
    decimal Amount,
    DateTime DueDate,
    string? Notes = null
) : IRequest<Result<Guid>>;
