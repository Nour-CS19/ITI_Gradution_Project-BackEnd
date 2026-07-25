using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Commands.Checkout
{
    public record CheckoutCommand(Guid UserId)
    : IRequest<Guid>;
}
