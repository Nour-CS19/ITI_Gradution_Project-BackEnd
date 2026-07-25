using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAllProductsQuery, PagedResult<AdminProductDto>>
{
    public async Task<PagedResult<AdminProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking()
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .Include(p => p.SellerProfile)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var pageNumber = Math.Max(1, request.PageNumber);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminProductDto
            {
                Id = p.Id,
                SellerProfileId = p.SellerProfileId,
                Name = p.Name,
                MinPrice = p.ProductVariants.Any() ? p.ProductVariants.Min(v => v.Price) : 0m,
                IsPublished = p.IsPuplished,
                VariantCount = p.ProductVariants.Count,
                ImageCount = p.ProductImages.Count,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminProductDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
