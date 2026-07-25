using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Application.Features.MarketPlace.Products.Common;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetProductDetails
{
    public class GetProductDetailsQueryHandler(IAppDbContext db)
     : IRequestHandler<GetProductDetailsQuery, ProductDetailsDto>
    {
        public async Task<ProductDetailsDto> Handle(
            GetProductDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .AsNoTracking()
                .Include(x => x.ProductImages)
                .Include(x => x.ProductVariants)
                .Include(x => x.ProductCategory)
                .Include(x => x.SellerProfile)
                    .ThenInclude(sp => sp!.User)
                .FirstOrDefaultAsync(
                    x => x.Id == request.ProductId,
                    cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            var latest = await db.ApprovalRequests
                .AsNoTracking()
                .Where(a => a.Type == ApprovalEntityType.ProductApproval && a.EntityId == product.Id)
                .OrderByDescending(a => a.RequestedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var status = ProductStatusHelper.Resolve(product.IsPuplished, latest?.ApprovalStatus);

            return new ProductDetailsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory?.Name,
                IsPublished = product.IsPuplished,
                Status = status.ToString(),
                AdminNote = status == ProductStatus.Rejected
                    ? ApprovalNotePayload.Parse(latest?.Note).AdminNote
                    : null,

                Images = product.ProductImages
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => x.ImageUrl)
                    .ToList(),

                Variants = product.ProductVariants
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
                    .ToList(),

                SellerStoreName = product.SellerProfile?.StoreName,
                SellerName = product.SellerProfile?.User != null
                    ? $"{product.SellerProfile.User.FirstName} {product.SellerProfile.User.LastName}".Trim()
                    : null
            };
        }
    }
}
