using FluentValidation;

namespace Application.Features.POS.Commands;

public class ProcessReturnCommandValidator : AbstractValidator<ProcessReturnCommand>
{
    public ProcessReturnCommandValidator()
    {
        RuleFor(x => x.OriginalOrderId)
            .NotEmpty()
            .WithMessage("Original order ID is required");

        RuleFor(x => x.StoreId)
            .NotEmpty()
            .WithMessage("Store ID is required");

        RuleFor(x => x.ProcessedByUserId)
            .NotEmpty()
            .WithMessage("Processed by user ID is required");

        RuleFor(x => x.ReturnItems)
            .NotEmpty()
            .WithMessage("At least one return item is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Return reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");

        RuleForEach(x => x.ReturnItems).ChildRules(item =>
        {
            item.RuleFor(x => x.OrderItemId)
                .NotEmpty()
                .WithMessage("Order item ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Return quantity must be greater than 0");
        });
    }
}
