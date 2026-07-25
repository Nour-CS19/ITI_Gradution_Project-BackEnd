using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Femora.Application.Features.Subscriptions.Commands.UpgradeSubscription;
using Femora.Application.Features.Subscriptions.Common.Requests;
using Femora.Application.Features.Subscriptions.Queries.GetSubscriptionStatus;

namespace Femora.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upgrade")]
        [Authorize]
        public async Task<IActionResult> Upgrade([FromBody] UpgradeSubscriptionRequest request)
        {
            if (request is null)
                return BadRequest("Request body cannot be null.");

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var command = new UpgradeSubscriptionCommand
            {
                PlanId = request.PlanId,
                BillingCycle = request.BillingCycle,
                UserId = userId
            };

            var subscriptionId = await _mediator.Send(command);
            return Ok(new { subscriptionId });
        }

        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> Status()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var dto = await _mediator.Send(new GetSubscriptionStatusQuery { UserId = userId });
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }
    }
}
