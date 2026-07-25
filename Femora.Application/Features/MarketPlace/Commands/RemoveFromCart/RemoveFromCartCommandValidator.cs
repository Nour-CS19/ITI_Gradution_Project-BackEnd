using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandValidator
    : AbstractValidator<RemoveFromCartCommand>
    {
        public RemoveFromCartCommandValidator()
        {
            RuleFor(x => x.CartItemId)
                .NotEmpty();
        }
    }
}
