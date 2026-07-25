using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.PlaceOrder
{
    public record PlaceOrderCommand : IRequest<Guid>
    {
        public Guid UserId { get; init; }
    }
}
