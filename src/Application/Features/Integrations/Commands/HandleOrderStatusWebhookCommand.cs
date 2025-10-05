using Application.Common.Models;
using MediatR;

namespace Application.Features.Integrations.Commands;

public record HandleOrderStatusWebhookCommand(
    string Provider,
    string TrackingNumber,
    string Status,
    string Signature,
    Dictionary<string, object>? AdditionalData = null
) : IRequest<Result<bool>>;
