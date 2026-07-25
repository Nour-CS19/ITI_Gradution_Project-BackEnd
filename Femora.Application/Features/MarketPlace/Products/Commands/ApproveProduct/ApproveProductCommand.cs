using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Commands.ApproveProduct
{

    public record ApproveProductCommand(Guid ProductId, Guid AdminId)
        : IRequest;
}
