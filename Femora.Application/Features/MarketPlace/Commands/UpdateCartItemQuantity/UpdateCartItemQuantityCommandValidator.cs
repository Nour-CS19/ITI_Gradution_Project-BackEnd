using FluentValidation;

namespace Femora.Application.Features.MarketPlace.Commands.UpdateCartItemQuantity
{
    public class UpdateCartItemQuantityCommandValidator
        : AbstractValidator<UpdateCartItemQuantityCommand>
    {
        public UpdateCartItemQuantityCommandValidator()
        {
            RuleFor(x => x.CartItemId)
                .NotEmpty();

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(100); // matches CartItem.Quantity [Range(1,100)] domain constraint
        }
    }
}
