using FluentValidation;

namespace Application.Features.POS.Commands;

public class CompleteOrderCommandValidator : AbstractValidator<CompleteOrderCommand>
{
    public CompleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.ProcessedByUserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
