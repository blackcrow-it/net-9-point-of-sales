using Application.Common.Models;
using MediatR;

namespace Application.Features.POS.Commands;

public record HoldOrderCommand(Guid OrderId) : IRequest<Result<Guid>>;
