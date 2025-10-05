using Application.Common.Models;
using MediatR;

namespace Application.Features.Stores.Queries;

public record GetSuppliersQuery(
    bool? IsActive = null
) : IRequest<Result<List<SupplierDto>>>;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive,
    DateTime CreatedAt
);
