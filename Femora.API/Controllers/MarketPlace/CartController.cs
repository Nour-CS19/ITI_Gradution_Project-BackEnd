using Femora.Application.Features.MarketPlace.Commands.AddToCart;
using Femora.Application.Features.MarketPlace.Commands.RemoveFromCart;
using Femora.Application.Features.MarketPlace.Commands.UpdateCartItemQuantity;
using Femora.Application.Features.MarketPlace.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.MarketPlace
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController(IMediator mediator) : ControllerBase
    {
        [HttpPost("add")]
        public async Task<IActionResult> Add(AddToCartCommand command, CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> Remove([FromQuery] RemoveFromCartCommand command, CancellationToken ct)
        {
            await mediator.Send(command, ct);
            return NoContent();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity(UpdateCartItemQuantityCommand command, CancellationToken ct)
        {
            await mediator.Send(command, ct);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetCartQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }
    }
}
