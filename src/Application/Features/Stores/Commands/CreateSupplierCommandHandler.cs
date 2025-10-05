using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Store;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Stores.Commands;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        // Validate Code uniqueness
        var codeExists = await _context.Suppliers
            .AnyAsync(s => s.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            return Result<Guid>.Failure($"Supplier with code '{request.Code}' already exists");
        }

        // Create supplier
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplier.Id);
    }
}
