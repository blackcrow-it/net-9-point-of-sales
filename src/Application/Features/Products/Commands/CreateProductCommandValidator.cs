using FluentValidation;

namespace Application.Features.Products.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200);

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50)
            .Matches("^[A-Z0-9-]+$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be > 0");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).When(x => x.Cost.HasValue)
            .WithMessage("Cost must be >= 0");

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");
    }
}
