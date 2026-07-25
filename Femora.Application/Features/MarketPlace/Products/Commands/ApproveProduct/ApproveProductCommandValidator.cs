using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Commands.ApproveProduct
{
    public class ApproveProductCommandValidator
     : AbstractValidator<ApproveProductCommand>
    {
        public ApproveProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();

            RuleFor(x => x.AdminId)
                .NotEmpty();
        }
    }
}
