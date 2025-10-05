using Application.Common.Models;
using MediatR;

namespace Application.Features.Customers.Commands;

public record CreateCustomerCommand(
    string Code,
    string Name,
    string? Phone,
    string? Email,
    Guid? CustomerGroupId,
    string? Address,
    Guid StoreId,
    bool IsActive = true
) : IRequest<Result<Guid>>;
