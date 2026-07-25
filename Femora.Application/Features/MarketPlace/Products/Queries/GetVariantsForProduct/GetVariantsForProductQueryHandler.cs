using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetVariantsForProduct
{
    public class GetVariantsForProductQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<GetVariantsForProductQuery, List<ProductVariantDto>>
    {
        public async Task<List<ProductVariantDto>> Handle(
            GetVariantsForProductQuery request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .AsNoTracking()
                .Include(p => p.SellerProfile)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            if (product.SellerProfile is null || product.SellerProfile.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            return product.ProductVariants
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material
                })
                .ToList();
        }
    }
}
