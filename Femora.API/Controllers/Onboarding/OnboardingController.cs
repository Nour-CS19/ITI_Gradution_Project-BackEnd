using Femora.Application.Features.Onboarding.Commands.SetGoal;
using Femora.Application.Features.Onboarding.Queries.GetGoals;
using Femora.Application.Features.Onboarding.Queries.GetInterests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Femora.API.Controllers.Onboarding
{
    [Route("api/onboarding")]
    [ApiController]
    public class OnboardingController(IMediator mediator) : ControllerBase
    {
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User id claim not found."));

        /// <summary>
        /// Gets all available onboarding interests/categories.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of onboarding interests with Arabic and English names.</returns>
        [HttpGet("interests")]
        [ProducesResponseType(typeof(List<OnboardingInterestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInterests(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetOnboardingInterestsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets all available onboarding goals.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of onboarding goals with Arabic and English labels.</returns>
        [HttpGet("goals")]
        [ProducesResponseType(typeof(List<OnboardingGoalDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGoals(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetOnboardingGoalsQuery(), cancellationToken);
            return Ok(result);
        }


        [HttpPost("goal")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveGoal([FromBody] SaveOnboardingGoalRequest request, CancellationToken cancellationToken)
        {
            await mediator.Send(new SetOnboardingGoalCommand
            {
                UserId = CurrentUserId,
                GoalId = request.GoalId
            }, cancellationToken);

            return Ok();
        }
    }

    public sealed record SaveOnboardingGoalRequest(Guid GoalId);
}
