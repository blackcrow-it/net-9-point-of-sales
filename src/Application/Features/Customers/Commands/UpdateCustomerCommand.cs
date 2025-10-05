using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Commands;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    Guid? CustomerGroupId,
    string? Address,
    bool IsActive
) : IRequest<Result>;
