using Application.Common.Models;
using MediatR;

namespace Application.Features.Stores.Commands;

public record CreateSupplierCommand(
    string Code,
    string Name,
    string? ContactPerson = null,
    string? Phone = null,
    string? Email = null,
    string? Address = null,
    bool IsActive = true
) : IRequest<Result<Guid>>;
