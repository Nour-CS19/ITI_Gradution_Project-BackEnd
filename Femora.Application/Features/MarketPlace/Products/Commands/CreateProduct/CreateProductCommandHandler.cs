using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.MarketPlace.Products.Common;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Femora.Application.Features.MarketPlace.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IBlobStorageRepository blobStorage)
     : IRequestHandler<CreateProductCommand, Guid>
    {
        public async Task<Guid> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            var sellerProfileId = await db.SellerProfiles
                .AsNoTracking()
                .Where(sp => sp.UserId == currentUser.UserId)
                .Select(sp => (Guid?)sp.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sellerProfileId is null)
                throw new NotFoundException("SellerProfile", currentUser.UserId.ToString());

            var categoryExists = await db.ProductCategories
                .AnyAsync(x => x.Id == request.ProductCategoryId, cancellationToken);

            if (!categoryExists)
                throw new NotFoundException("ProductCategory", request.ProductCategoryId.ToString());

            var variants = ParseVariants(request.VariantsJson);
            if (variants.Count == 0)
                throw new InvalidOperationException("At least one product variant is required.");

            // Product is created as a Draft: not published, no approval request yet.
            // The seller submits it for review explicitly via PublishProduct.
            var product = new Product
            {
                SellerProfileId = sellerProfileId.Value,
                ProductCategoryId = request.ProductCategoryId,
                Name = request.Name,
                Description = request.Description,
                IsPuplished = false
            };

            foreach (var v in variants)
            {
                product.ProductVariants.Add(new ProductVariant
                {
                    Name = v.Name,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material
                });
            }

            if (request.Images is not null && request.Images.Count > 0)
            {
                var orderIndex = 0;
                foreach (var image in request.Images)
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
                        ImageUrl = imageUrl,
                        IsPrimary = orderIndex == 0,
                        OrderIndex = orderIndex
                    });

                    orderIndex++;
                }
            }

            db.Products.Add(product);

            await db.SaveChangesAsync(cancellationToken);

            return product.Id;
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
