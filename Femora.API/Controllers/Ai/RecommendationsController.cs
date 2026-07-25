using Femora.Application.Features.AI.Recommendations.Commands.SuggestProductImprovements;
using Femora.Application.Features.AI.Recommendations.Commands.SuggestProductPrice;
using Femora.Application.Features.AI.Recommendations.Queries.RecommendCourses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.AI;

/// <summary>
/// AI-powered recommendations: course recommendations for trainees,
/// and pricing / listing-quality suggestions for sellers.
/// </summary>
[Route("api/recommendations")]
[ApiController]
[Authorize]
[Tags("AI Recommendations")]
public class RecommendationsController(IMediator mediator) : ControllerBase
{
    // ---------------------------------------------------------------
    // Trainee: course recommendations (data-driven, no AI call)
    // ---------------------------------------------------------------

    /// <summary>
    /// Recommends courses for a trainee based on their skill level,
    /// learning goals, and preferred categories.
    /// </summary>
    [HttpGet("trainees/{traineeProfileId:guid}/courses")]
    [ProducesResponseType(typeof(RecommendCoursesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecommendCourses(
        [FromRoute] Guid traineeProfileId,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new RecommendCoursesQuery { TraineeProfileId = traineeProfileId, MaxResults = maxResults },
            cancellationToken);

        return Ok(result);
    }

    // ---------------------------------------------------------------
    // Seller: price suggestion (market data + AI)
    // ---------------------------------------------------------------

    /// <summary>
    /// Suggests a competitive price for a seller's product, based on
    /// market data from similar products plus an AI reasoning pass.
    /// </summary>
    [HttpPost("products/{productId:guid}/price-suggestion")]
    [ProducesResponseType(typeof(SuggestProductPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuggestProductPrice(
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SuggestProductPriceCommand { ProductId = productId },
            cancellationToken);

        return Ok(result);
    }

    // ---------------------------------------------------------------
    // Seller: listing/quality improvement suggestions (AI text)
    // ---------------------------------------------------------------

    /// <summary>
    /// Generates AI suggestions to improve a product listing's quality
    /// (description, images, variant completeness, etc.).
    /// </summary>
    [HttpPost("products/{productId:guid}/quality-suggestions")]
    [ProducesResponseType(typeof(SuggestProductImprovementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuggestProductImprovements(
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SuggestProductImprovementsCommand { ProductId = productId },
            cancellationToken);

        return Ok(result);
    }
}
