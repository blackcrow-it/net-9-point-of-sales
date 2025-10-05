using FluentValidation;

namespace Application.Features.POS.Commands;

public class StartShiftCommandValidator : AbstractValidator<StartShiftCommand>
{
    public StartShiftCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.OpeningCash)
            .GreaterThanOrEqualTo(0).WithMessage("Opening cash must be >= 0");
    }
}
