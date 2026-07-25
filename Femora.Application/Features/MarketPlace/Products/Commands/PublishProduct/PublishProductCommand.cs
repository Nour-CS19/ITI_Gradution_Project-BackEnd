using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Commands.PublishProduct
{
    public record PublishProductCommand(Guid ProductId)
     : IRequest;
}
