using FluentValidation;

namespace Femora.Application.Features.MarketPlace.Products.Commands.AddVariant
{
    public class AddVariantCommandValidator : AbstractValidator<AddVariantCommand>
    {
        public AddVariantCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Color)
                .MaximumLength(50);

            RuleFor(x => x.Size)
                .MaximumLength(50);

            RuleFor(x => x.Material)
                .MaximumLength(100);
        }
    }
}
