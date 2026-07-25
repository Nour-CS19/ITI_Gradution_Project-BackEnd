using FluentValidation;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator
    : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.ProductCategoryId)
                .NotEmpty();

            RuleFor(x => x.VariantsJson)
                .NotEmpty()
                .WithMessage("At least one product variant is required.");
        }
    }
}
