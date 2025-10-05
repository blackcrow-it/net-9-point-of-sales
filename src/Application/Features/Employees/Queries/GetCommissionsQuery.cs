using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Queries;

public record GetCommissionsQuery(
    Guid EmployeeId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<CommissionDto>>>;

public record CommissionDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeFullName,
    Guid OrderId,
    string? OrderNumber,
    decimal OrderAmount,
    decimal CommissionRate,
    decimal CommissionAmount,
    DateTime CalculatedDate,
    string Status
);
