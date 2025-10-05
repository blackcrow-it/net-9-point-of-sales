using FluentValidation;

namespace Application.Features.Inventory.Commands;

public class CreateInventoryReceiptCommandValidator : AbstractValidator<CreateInventoryReceiptCommand>
{
    public CreateInventoryReceiptCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required");

        RuleFor(x => x.ReceiptNumber)
            .NotEmpty().WithMessage("Receipt number is required")
            .MaximumLength(50);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Receipt must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be > 0");

            item.RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("Unit cost must be >= 0");
        });
    }
}
