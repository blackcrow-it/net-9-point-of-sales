using FluentValidation;

namespace Application.Features.POS.Commands;

public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty().WithMessage("Payment method ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be > 0");
    }
}
