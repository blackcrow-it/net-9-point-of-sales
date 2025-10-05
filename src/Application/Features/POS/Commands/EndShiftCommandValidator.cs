using FluentValidation;

namespace Application.Features.POS.Commands;

public class EndShiftCommandValidator : AbstractValidator<EndShiftCommand>
{
    public EndShiftCommandValidator()
    {
        RuleFor(x => x.ShiftId)
            .NotEmpty().WithMessage("Shift ID is required");

        RuleFor(x => x.ClosingCash)
            .GreaterThanOrEqualTo(0).WithMessage("Closing cash must be >= 0");
    }
}
