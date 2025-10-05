using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Commands;

public record CreateCommissionCommand(
    Guid EmployeeId,
    Guid OrderId,
    decimal OrderAmount,
    decimal CommissionRate
) : IRequest<Result<Guid>>;
