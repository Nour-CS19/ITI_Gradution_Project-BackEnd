using Femora.API.Requests;
using Femora.Application.Features.Enrollments.Commands.Enroll;
using Femora.Application.Features.Enrollments.Commands.UnlockNextModule;
using Femora.Application.Features.Enrollments.Queries.EnrollmentDetails;
using Femora.Application.Features.Enrollments.Queries.GetMyEnrollments;
using Femora.Application.Features.Enrollments.Queries.IsEnrolled;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.LMS;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequestBody body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new EnrollCommand(body.CourseId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(Policy ="Trainee")]
    public async Task<IActionResult> GetMyEnrollments(
        [FromQuery] GetMyEnrollmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{enrollmentId:guid}")]
    public async Task<IActionResult> GetEnrollmentDetails(
      Guid enrollmentId,
      CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new EnrollmentDetailsQuery(enrollmentId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("is-enrolled/{courseId:guid}")]
    public async Task<IActionResult> IsEnrolled(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IsEnrolledQuery(courseId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("unlock-next-module")]
    [Authorize(Policy = "Trainee")]
    public async Task<IActionResult> UnlockNextModule(
        [FromBody] UnlockNextModuleRequestBody body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnlockNextModuleCommand(body.CurrentModuleId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("lessons/{lessonId:guid}/complete")]
    [Authorize(Policy = "Trainee")]
    public async Task<IActionResult> CompleteLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new Femora.Application.Features.Enrollments.Commands.CompleteLesson.CompleteLessonCommand(lessonId), cancellationToken);
        return Ok(result);
    }
}
