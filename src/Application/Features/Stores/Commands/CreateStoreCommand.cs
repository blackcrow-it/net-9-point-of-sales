using Application.Common.Models;
using Domain.Entities.Store;
using MediatR;

namespace Application.Features.Stores.Commands;

public record CreateStoreCommand(
    string Code,
    string Name,
    StoreType Type,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    bool IsActive = true
) : IRequest<Result<Guid>>;
