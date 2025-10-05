using Application.Common.Models;
using MediatR;

namespace Application.Features.Inventory.Commands;

/// <summary>
/// Command to complete an inventory issue and update stock levels
/// </summary>
public record CompleteInventoryIssueCommand(
    Guid IssueId
) : IRequest<Result<Guid>>;
