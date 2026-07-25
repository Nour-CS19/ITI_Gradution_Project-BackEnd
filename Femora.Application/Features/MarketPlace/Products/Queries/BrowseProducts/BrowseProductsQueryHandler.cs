using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Queries.BrowseProducts
{
    public class BrowseProductsQueryHandler(IAppDbContext db)
     : IRequestHandler<BrowseProductsQuery, PagedResult<ProductSummaryDto>>
    {
        public async Task<PagedResult<ProductSummaryDto>> Handle(
            BrowseProductsQuery request,
            CancellationToken cancellationToken)
        {
            var query = db.Products
                .AsNoTracking()
                .Where(p => p.IsPuplished);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p =>
                    p.Name.Contains(request.Search));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategoryId == request.CategoryId.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,

                    ImageUrl = p.ProductImages
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    ImageUrls = p.ProductImages
                        .OrderBy(i => i.OrderIndex)
                        .Select(i => i.ImageUrl)
                        .Take(3)
                        .ToList(),

                    MinPrice = p.ProductVariants.Any()
                        ? p.ProductVariants.Min(v => v.Price)
                        : 0,

                    CategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : null
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ProductSummaryDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = total
            };
        }
    }
}
