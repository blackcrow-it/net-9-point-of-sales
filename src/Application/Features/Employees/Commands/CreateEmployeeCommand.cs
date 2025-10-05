using Application.Common.Models;
using MediatR;

namespace Application.Features.Employees.Commands;

/// <summary>
/// Command to create new employee with hashed password
/// </summary>
public record CreateEmployeeCommand(
    string Username,
    string Password,
    string FullName,
    string Email,
    string? Phone,
    Guid StoreId,
    Guid RoleId,
    bool IsActive = true
) : IRequest<Result<Guid>>;
