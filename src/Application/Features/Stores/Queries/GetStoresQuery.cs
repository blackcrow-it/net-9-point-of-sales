using Application.Common.Models;
using Domain.Entities.Store;
using MediatR;

namespace Application.Features.Stores.Queries;

public record GetStoresQuery(
    bool? IsActive = null
) : IRequest<Result<List<StoreDto>>>;

public record StoreDto(
    Guid Id,
    string Code,
    string Name,
    StoreType Type,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive,
    DateTime CreatedAt
);
