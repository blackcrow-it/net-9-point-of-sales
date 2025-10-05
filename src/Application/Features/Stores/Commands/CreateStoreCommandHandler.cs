using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Store;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stores.Commands;

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateStoreCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        // Validate Code uniqueness
        var codeExists = await _context.Stores
            .AnyAsync(s => s.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Failure($"Store with code '{request.Code}' already exists");
        }

        // Create store
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Type = request.Type,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Stores.Add(store);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(store.Id);
    }
}
