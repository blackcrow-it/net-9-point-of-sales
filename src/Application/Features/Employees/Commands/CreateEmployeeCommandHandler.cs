using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Validate username uniqueness
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
            return Result<Guid>.Failure("Username already exists");

        // Validate email uniqueness
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            return Result<Guid>.Failure("Email already exists");

        // Verify role exists
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Id == request.RoleId, cancellationToken);

        if (!roleExists)
            return Result<Guid>.Failure("Role not found");

        // Verify store exists
        var storeExists = await _context.Stores
            .AnyAsync(s => s.Id == request.StoreId, cancellationToken);

        if (!storeExists)
            return Result<Guid>.Failure("Store not found");

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            StoreId = request.StoreId,
            RoleId = request.RoleId,
            IsActive = request.IsActive
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
