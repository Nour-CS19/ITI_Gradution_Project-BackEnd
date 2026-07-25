using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Products.Commands.CreateProduct
{
    /// <summary>
    /// Creates a product as a Draft (SellerProfileId is resolved server-side from the
    /// authenticated seller — never trusted from the client).
    /// VariantsJson is a JSON-serialized List&lt;ProductVariantInput&gt; because this
    /// endpoint accepts multipart/form-data (for Images) where nested complex lists
    /// can't be model-bound directly.
    /// </summary>
    public record CreateProductCommand(
        string Name,
        string? Description,
        Guid ProductCategoryId,
        string VariantsJson
    ) : IRequest<Guid>
    {
        public List<IFormFile>? Images { get; init; }
    }
}
