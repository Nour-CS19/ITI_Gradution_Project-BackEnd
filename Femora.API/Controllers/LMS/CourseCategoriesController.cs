using Femora.Application.Features.LMS.Categories.Queries.GetCourseCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.LMS;

/// <summary>
/// Public, read-only catalog of course categories - used to populate the
/// category picker on the onboarding / "edit my interests" screen.
/// </summary>
[Route("api/course-categories")]
[ApiController]
public class CourseCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCourseCategoriesQuery(), ct);
        return Ok(result);
    }
}
