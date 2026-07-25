using Femora.Domain.Entities.Marketplace;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Queries.GetMyOrders
{
    public record GetMyOrdersQuery : IRequest<IEnumerable<Order>>
    {
    }
}
