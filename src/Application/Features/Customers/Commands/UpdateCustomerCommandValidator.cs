using FluentValidation;

namespace Application.Features.Customers.Commands;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .Matches(@"^0\d{9}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be 10 digits starting with 0");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");
    }
}
