using FluentValidation;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateVariant
{
    public class UpdateVariantCommandValidator : AbstractValidator<UpdateVariantCommand>
    {
        public UpdateVariantCommandValidator()
        {
            RuleFor(x => x.VariantId)
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
