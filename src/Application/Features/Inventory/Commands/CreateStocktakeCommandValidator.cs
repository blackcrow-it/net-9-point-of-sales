using FluentValidation;

namespace Application.Features.Inventory.Commands;

public class CreateStocktakeCommandValidator : AbstractValidator<CreateStocktakeCommand>
{
    public CreateStocktakeCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("Reference is required")
            .MaximumLength(50);

        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("At least one product must be selected for stocktake");
    }
}
