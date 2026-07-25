using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetVariantsForProduct
{
    /// <summary>
    /// Returns all variants for a product identified by productId.
    /// Seller-only endpoint; validates ownership.
    /// </summary>
    public record GetVariantsForProductQuery(Guid ProductId) : IRequest<List<ProductVariantDto>>;
}
