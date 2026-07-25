using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Queries.BrowseProducts
{
    public class BrowseProductsQueryHandler(IAppDbContext db)
     : IRequestHandler<BrowseProductsQuery, List<Product>>
    {
        public async Task<List<Product>> Handle(
            BrowseProductsQuery request,
            CancellationToken cancellationToken)
        {
            return await db.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                .Where(x => x.IsPuplished)
                .ToListAsync(cancellationToken);
        }
    }
}
