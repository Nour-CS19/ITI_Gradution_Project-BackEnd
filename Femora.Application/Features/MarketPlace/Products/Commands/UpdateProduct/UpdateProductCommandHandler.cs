using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.MarketPlace.Products.Common;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IBlobStorageRepository blobStorage)
     : IRequestHandler<UpdateProductCommand>
    {
        public async Task Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductImages)
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            if (product.SellerProfile is null || product.SellerProfile.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var hasPendingApproval = await db.ApprovalRequests.AnyAsync(
                x => x.EntityId == product.Id
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

            if (hasPendingApproval)
                throw new InvalidOperationException(
                    "This product is awaiting admin review and can't be edited right now.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.ProductCategoryId = request.ProductCategoryId;

            var variants = ParseVariants(request.VariantsJson);
            if (variants.Count == 0)
                throw new InvalidOperationException("At least one product variant is required.");

            db.ProductVariants.RemoveRange(product.ProductVariants);
            product.ProductVariants.Clear();
            foreach (var v in variants)
            {
                product.ProductVariants.Add(new ProductVariant
                {
                    ProductId = product.Id,
                    Name = v.Name,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material
                });
            }

            if (request.NewImages is not null && request.NewImages.Count > 0)
            {
                var orderIndex = product.ProductImages.Count == 0
                    ? 0
                    : product.ProductImages.Max(i => i.OrderIndex) + 1;

                foreach (var image in request.NewImages)
                {
                    await using var stream = image.OpenReadStream();
                    var imageUrl = await blobStorage.UploadFileAsync(
                        stream,
                        image.FileName,
                        image.ContentType,
                        folder: "product-images",
                        cancellationToken: cancellationToken);

                    product.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = !product.ProductImages.Any(i => i.IsPrimary),
                        OrderIndex = orderIndex
                    });

                    orderIndex++;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static List<ProductVariantInput> ParseVariants(string variantsJson)
        {
            if (string.IsNullOrWhiteSpace(variantsJson))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<ProductVariantInput>>(
                    variantsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Variants must be valid JSON.");
            }
        }
    }
}
