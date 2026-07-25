using Femora.Application.Features.MarketPlace.Products.Commands.AddVariant;
using Femora.Application.Features.MarketPlace.Products.Commands.ApproveProduct;
using Femora.Application.Features.MarketPlace.Products.Commands.CreateProduct;
using Femora.Application.Features.MarketPlace.Products.Commands.DeleteProductImage;
using Femora.Application.Features.MarketPlace.Products.Commands.DeleteVariant;
using Femora.Application.Features.MarketPlace.Products.Commands.PublishProduct;
using Femora.Application.Features.MarketPlace.Products.Commands.SetPrimaryImage;
using Femora.Application.Features.MarketPlace.Products.Commands.UpdateProduct;
using Femora.Application.Features.MarketPlace.Products.Commands.UpdateVariant;
using Femora.Application.Features.MarketPlace.Products.Queries.BrowseProducts;
using Femora.Application.Features.MarketPlace.Products.Queries.GetMyProducts;
using Femora.Application.Features.MarketPlace.Products.Queries.GetProductDetails;
using Femora.Application.Features.MarketPlace.Products.Queries.GetProductImages;
using Femora.Application.Features.MarketPlace.Products.Queries.GetSellerStats;
using Femora.Application.Features.MarketPlace.Products.Queries.GetVariantsForProduct;
using MediatR;
using Femora.Application.Features.Identity.Common.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

namespace Femora.API.Controllers.Marketplace
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController(IMediator mediator) : ControllerBase
    {
        // ─── Public browsing ──────────────────────────────────────────────────

        [HttpGet]
        [OutputCache(PolicyName = "Listings")]
        public async Task<IActionResult> BrowseProducts([FromQuery] BrowseProductsQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductDetails(Guid id)
        {
            var result = await mediator.Send(new GetProductDetailsQuery(id));
            return Ok(result);
        }

        // ─── Seller product management ────────────────────────────────────────

        /// <summary>Seller's own products — list/search/filter, all statuses.</summary>
        [HttpGet("mine")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> GetMyProducts([FromQuery] GetMyProductsQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        /// <summary>Seller dashboard KPIs: product counts, order counts, revenue, best sellers, latest orders.</summary>
        [HttpGet("mine/stats")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> GetSellerStats()
        {
            var result = await mediator.Send(new GetSellerStatsQuery());
            return Ok(result);
        }

        /// <summary>Creates a product as a Draft. multipart/form-data.</summary>
        [HttpPost]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductCommand command)
        {
            var productId = await mediator.Send(command);
            return Ok(productId);
        }

        /// <summary>Edits a Draft/Rejected product. multipart/form-data.</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] UpdateProductCommand command)
        {
            command = command with { ProductId = id };
            await mediator.Send(command);
            return NoContent();
        }

        /// <summary>Submits a Draft/Rejected product for admin review.</summary>
        [HttpPost("{id:guid}/publish")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> PublishProduct(Guid id)
        {
            await mediator.Send(new PublishProductCommand(id));
            return NoContent();
        }

        // ─── Admin approval ───────────────────────────────────────────────────

        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveProduct(Guid id)
        {
            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
                return Unauthorized();

            await mediator.Send(new ApproveProductCommand(id, adminId));
            return NoContent();
        }

        // ─── Variant endpoints ────────────────────────────────────────────────

        /// <summary>Returns all variants for a seller's product.</summary>
        [HttpGet("{id:guid}/variants")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> GetVariants(Guid id)
        {
            var result = await mediator.Send(new GetVariantsForProductQuery(id));
            return Ok(result);
        }

        /// <summary>Adds a new variant to a Draft/Rejected product.</summary>
        [HttpPost("{id:guid}/variants")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> AddVariant(Guid id, [FromBody] AddVariantCommand command)
        {
            command = command with { ProductId = id };
            var variantId = await mediator.Send(command);
            return Ok(variantId);
        }

        /// <summary>Updates an existing variant.</summary>
        [HttpPut("{id:guid}/variants/{variantId:guid}")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> UpdateVariant(Guid id, Guid variantId, [FromBody] UpdateVariantCommand command)
        {
            command = command with { VariantId = variantId };
            await mediator.Send(command);
            return NoContent();
        }

        /// <summary>Removes a variant (product must retain at least one variant).</summary>
        [HttpDelete("{id:guid}/variants/{variantId:guid}")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> DeleteVariant(Guid id, Guid variantId)
        {
            await mediator.Send(new DeleteVariantCommand(variantId));
            return NoContent();
        }

        // ─── Image endpoints ──────────────────────────────────────────────────

        /// <summary>Returns all images for a seller's product with their IDs.</summary>
        [HttpGet("{id:guid}/images")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> GetImages(Guid id)
        {
            var result = await mediator.Send(new GetProductImagesQuery(id));
            return Ok(result);
        }

        /// <summary>Deletes a single image (must retain ≥1 image).</summary>
        [HttpDelete("{id:guid}/images/{imageId:guid}")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
        {
            await mediator.Send(new DeleteProductImageCommand(imageId));
            return NoContent();
        }

        /// <summary>Sets an image as the primary (cover) image for the product.</summary>
        [HttpPatch("{id:guid}/images/{imageId:guid}/set-primary")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> SetPrimaryImage(Guid id, Guid imageId)
        {
            await mediator.Send(new SetPrimaryImageCommand(imageId));
            return NoContent();
        }
    }
}
