using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Queries.GetMyOrders
{
    public class GetMyOrdersQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyOrdersQuery, IEnumerable<Order>>
    {
        public async Task<IEnumerable<Order>> Handle(
            GetMyOrdersQuery request,
            CancellationToken cancellationToken)
        {
            return await db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == currentUser.UserId)
                .ToListAsync(cancellationToken);
        }
    }
}