using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;

using Femora.Application.Features.LMS.Courses.Commands;
using Femora.Application.Features.LMS.Courses.Commands.ApproveCourse;
using Femora.Application.Features.LMS.Courses.Queries;

namespace Femora.API.Controllers.LMS;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // =========================
    // 📌 GET ALL COURSES
    // =========================
    [HttpGet]
    [OutputCache(PolicyName = "Listings")]
    public async Task<IActionResult> GetAll([FromQuery] GetCoursesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // =========================
    // 📌 GET COURSE BY ID
    // =========================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid? userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId)
            ? parsedId
            : null;
        var isAdmin = User.IsInRole("Admin");

        var result = await _mediator.Send(new GetCourseByIdQuery(id, userId, isAdmin));
        return Ok(result);
    }

    // =========================
    // 📌 GET MY COURSES (Instructor)
    // =========================
    [HttpGet("my")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> GetMyCourses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var instructorId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyCoursesQuery
        {
            UserId = instructorId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        return Ok(result);
    }

    // =========================
    // 📌 INSTRUCTOR DASHBOARD STATS
    // =========================
    [HttpGet("my/stats")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> GetMyDashboardStats()
    {
        var result = await _mediator.Send(new GetInstructorDashboardStatsQuery());
        return Ok(result);
    }

    // =========================
    // 📌 CREATE COURSE
    // =========================
    [HttpPost]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CreateCourseCommand command)
    {
        var courseId = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetById),
            new { id = courseId },
            courseId);
    }

    // =========================
    // 📌 UPDATE COURSE
    // =========================
    [HttpPut("{id}")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Update(
      Guid id,
      [FromBody] UpdateCourseCommand command)
    {
        if (id != command.CourseId)
            return BadRequest();

        await _mediator.Send(command);

        return NoContent();
    }
    // =========================
    // 📌 DELETE COURSE
    // =========================
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedAdminId))
            return Unauthorized();

        await _mediator.Send(new DeleteCourseCommand(id, parsedAdminId));

        return NoContent();
    }

    // =========================
    // 📌 UNPUBLISH COURSE
    // =========================
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
            return Unauthorized();

        await _mediator.Send(new UnpublishCourseCommand(id, parsedUserId));

        return Ok(new { message = "Course unpublished successfully" });
    }

    // =========================
    // 📌 ARCHIVE COURSE
    // =========================
    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
            return Unauthorized();

        await _mediator.Send(new ArchiveCourseCommand(id, parsedUserId));

        return Ok(new { message = "Course archived successfully" });
    }

    // =========================
    // 📌 PUBLISH COURSE
    // =========================
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var instructorId))
            return Unauthorized();

        await _mediator.Send(new PublishCourseCommand(id, instructorId));

        return Ok(new { message = "Course published successfully" });
    }

    // =========================
    // 📌 APPROVE COURSE (Admin)
    // =========================
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(adminId, out var parsedAdminId))
            return Unauthorized();

        await _mediator.Send(new ApproveCourseCommand(id, parsedAdminId));

        return Ok(new { message = "Course approved successfully" });
    }

    // =========================
    // 📌 SUBMIT COURSE FOR REVIEW
    // =========================
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "Instructor")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var parsedUserId))
            return Unauthorized();

        await _mediator.Send(new SubmitCourseCommand(id, parsedUserId));

        return Ok(new { message = "Course submitted for review successfully" });
    }

    public record RejectCourseRequest(string Reason);

    // =========================
    // 📌 REJECT COURSE (Admin)
    // =========================
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectCourseRequest request)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(adminId, out var parsedAdminId))
            return Unauthorized();

        await _mediator.Send(new RejectCourseCommand(id, parsedAdminId, request.Reason));

        return Ok(new { message = "Course rejected successfully" });
    }

    [HttpGet("filter-options")]
    [OutputCache(PolicyName = "StaticLookups")]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result =
            await _mediator.Send(new GetCourseFilterOptionsQuery());

        return Ok(result);
    }
}