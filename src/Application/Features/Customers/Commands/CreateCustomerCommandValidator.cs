using FluentValidation;

namespace Application.Features.Customers.Commands;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Customer code is required")
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .Matches(@"^0\d{9}$").When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be 10 digits starting with 0");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");
    }
}
