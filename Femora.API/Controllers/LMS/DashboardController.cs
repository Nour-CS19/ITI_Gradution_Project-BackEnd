using Femora.Application.Features.LMS.Dashboard.DTOs;
using Femora.Application.Features.LMS.Dashboard.Queries.GetTraineeDashboardStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.LMS;

[Route("api/dashboard")]
[ApiController]
[Authorize]
[Tags("Dashboard")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Everything the trainee "لوحة التحكم" screen needs in one call: stat cards
    /// (pending requests, exams, completed/ongoing courses), real per-course
    /// progress, the next quiz actually waiting on the trainee, and achievements —
    /// all computed live from Enrollments / QuizAttempts, nothing hardcoded.
    /// </summary>
    [HttpGet("trainee")]
    [Authorize(Policy = "Trainee")]
    [ProducesResponseType(typeof(TraineeDashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTraineeDashboard(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTraineeDashboardStatsQuery(), cancellationToken);
        return Ok(result);
    }
}
