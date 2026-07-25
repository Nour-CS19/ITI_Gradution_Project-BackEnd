using Femora.Application.Features.MarketPlace.Commands.PlaceOrder;
using Femora.Application.Features.MarketPlace.Orders.Commands.UpdateOrderStatus;
using Femora.Application.Features.MarketPlace.Orders.Queries.GetSellerOrders;
using Femora.Application.Features.MarketPlace.Queries.GetMyOrders;
using Femora.Application.Features.Identity.Common.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.MarketPlace
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderCommand command, CancellationToken ct)
        {
            var orderId = await mediator.Send(command, ct);
            return Ok(orderId);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] GetMyOrdersQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>Returns all orders that contain this seller's products.</summary>
        [HttpGet("seller")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> GetSellerOrders([FromQuery] GetSellerOrdersQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return Ok(result);
        }

        /// <summary>Seller can advance an order: Pending→Processing→Shipped→Delivered.</summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Policy = Policies.Seller)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest body, CancellationToken ct)
        {
            await mediator.Send(new UpdateOrderStatusCommand(id, body.Status), ct);
            return NoContent();
        }
    }

    public record UpdateStatusRequest(string Status);
}

