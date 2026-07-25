using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetProductImages
{
    public record ProductImageDto(Guid Id, string Url, bool IsPrimary, int OrderIndex);

    public record GetProductImagesQuery(Guid ProductId) : IRequest<List<ProductImageDto>>;

    public class GetProductImagesQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<GetProductImagesQuery, List<ProductImageDto>>
    {
        public async Task<List<ProductImageDto>> Handle(
            GetProductImagesQuery request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .AsNoTracking()
                .Include(p => p.SellerProfile)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            if (product.SellerProfile?.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            return product.ProductImages
                .OrderBy(i => i.OrderIndex)
                .Select(i => new ProductImageDto(i.Id, i.ImageUrl, i.IsPrimary, i.OrderIndex))
                .ToList();
        }
    }
}
