using FluentValidation;

namespace Femora.Application.Features.MarketPlace.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator
     : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.ProductCategoryId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.VariantsJson)
                .NotEmpty()
                .WithMessage("At least one product variant is required.");
        }
    }
}
