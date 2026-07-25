using Femora.Application.Common.Interfaces;
using Femora.Application.Features.MarketPlace.Categories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Categories.Queries.GetProductCategories
{
    public class GetProductCategoriesQueryHandler(IAppDbContext db)
        : IRequestHandler<GetProductCategoriesQuery, List<ProductCategoryDto>>
    {
        public async Task<List<ProductCategoryDto>> Handle(
            GetProductCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            return await db.ProductCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new ProductCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count(p => p.IsPuplished)
                })
                .ToListAsync(cancellationToken);
        }
    }
}
