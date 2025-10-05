using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Employees.Commands;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Validate role name uniqueness
        var nameExists = await _context.Roles
            .AnyAsync(r => r.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result<Guid>.Failure("Role name already exists");

        // Verify all permissions exist
        var permissions = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (permissions.Count != request.PermissionIds.Count)
            return Result<Guid>.Failure("One or more permissions not found");

        // Create role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            Permissions = permissions
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(role.Id);
    }
}
