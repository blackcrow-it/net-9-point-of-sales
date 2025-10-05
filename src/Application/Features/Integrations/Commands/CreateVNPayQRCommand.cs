using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Commands;

public record CreateVNPayQRCommand(
    Guid OrderId,
    decimal Amount
) : IRequest<Result<VNPayQRResponseDto>>;

public record VNPayQRResponseDto(
    string QRCodeUrl,
    string TransactionId,
    DateTime ExpiresAt
);
