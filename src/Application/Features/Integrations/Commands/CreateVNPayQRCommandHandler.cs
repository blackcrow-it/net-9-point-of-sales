using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Integrations.Commands;

public class CreateVNPayQRCommandHandler : IRequestHandler<CreateVNPayQRCommand, Result<VNPayQRResponseDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateVNPayQRCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<VNPayQRResponseDto>> Handle(CreateVNPayQRCommand request, CancellationToken cancellationToken)
    {
        // Verify order exists
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<VNPayQRResponseDto>.Failure("Order not found");
        }

        // TODO: Integrate with actual VNPay API
        // For now, return a mock response
        var transactionId = $"VNPAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
        var qrCodeUrl = $"https://sandbox.vnpayment.vn/qr/{transactionId}";
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var response = new VNPayQRResponseDto(
            qrCodeUrl,
            transactionId,
            expiresAt
        );

        // Store transaction reference (could be in Payment entity or separate IntegrationLog)
        // This is a placeholder - actual implementation would save to database
        
        return Result<VNPayQRResponseDto>.Success(response);
    }
}
