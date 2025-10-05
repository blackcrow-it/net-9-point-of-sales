using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Employees;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Employees.Commands;

public class CreateCommissionCommandHandler : IRequestHandler<CreateCommissionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCommissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCommissionCommand request, CancellationToken cancellationToken)
    {
        // Verify employee exists
        var employeeExists = await _context.Users
            .AnyAsync(u => u.Id == request.EmployeeId, cancellationToken);

        if (!employeeExists)
        {
            return Result<Guid>.Failure("Employee not found");
        }

        // Verify order exists
        var orderExists = await _context.Orders
            .AnyAsync(o => o.Id == request.OrderId, cancellationToken);

        if (!orderExists)
        {
            return Result<Guid>.Failure("Order not found");
        }

        // Create commission record
        var commission = new Commission
        {
            Id = Guid.NewGuid(),
            UserId = request.EmployeeId,
            OrderId = request.OrderId,
            OrderAmount = request.OrderAmount,
            CommissionRate = request.CommissionRate,
            CalculatedDate = DateTime.UtcNow,
            Status = Domain.Entities.Employees.CommissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate commission amount using entity method
        commission.CalculateCommissionAmount();

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(commission.Id);
    }
}
