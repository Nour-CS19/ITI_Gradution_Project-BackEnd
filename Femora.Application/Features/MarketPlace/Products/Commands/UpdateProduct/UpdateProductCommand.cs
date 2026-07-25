using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Edits a product's core fields and replaces its variant list.
    /// NewImages (if any) are appended to the existing gallery.
    /// Editing is only allowed while the product is Draft or Rejected — once a
    /// PendingApproval/Approved request exists it's locked until re-opened by a reject.
    /// </summary>
    public record UpdateProductCommand(
        Guid ProductId,
        string Name,
        string? Description,
        Guid ProductCategoryId,
        string VariantsJson
    ) : IRequest
    {
        public List<IFormFile>? NewImages { get; init; }
    }
}
