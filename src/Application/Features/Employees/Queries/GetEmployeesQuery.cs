using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Queries;

public record GetEmployeesQuery(
    Guid? StoreId,
    Guid? RoleId,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedList<EmployeeDto>>>;

public record EmployeeDto(
    Guid Id,
    string Username,
    string FullName,
    string? Email,
    string? Phone,
    Guid? StoreId,
    string? StoreName,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
