using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Commands;

/// <summary>
/// Command to add loyalty points to customer
/// </summary>
public record AddLoyaltyPointsCommand(
    Guid CustomerId,
    decimal Points,
    string Type,
    string Description,
    Guid? OrderId = null
) : IRequest<Result<Guid>>;
