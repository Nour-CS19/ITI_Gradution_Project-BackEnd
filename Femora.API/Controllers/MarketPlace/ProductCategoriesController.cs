using Femora.Application.Features.MarketPlace.Categories.Queries.GetProductCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Femora.API.Controllers.Marketplace
{
    /// <summary>
    /// Public, read-only catalog of product categories — used to populate the
    /// category filter dropdown on the marketplace product catalog page.
    /// </summary>
    [Route("api/product-categories")]
    [ApiController]
    public class ProductCategoriesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        [OutputCache(PolicyName = "StaticLookups")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await mediator.Send(new GetProductCategoriesQuery(), ct);
            return Ok(result);
        }
    }
}
