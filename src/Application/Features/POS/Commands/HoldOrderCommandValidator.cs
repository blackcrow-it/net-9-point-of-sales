using FluentValidation;

namespace Application.Features.POS.Commands;

public class HoldOrderCommandValidator : AbstractValidator<HoldOrderCommand>
{
    public HoldOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");
    }
}
