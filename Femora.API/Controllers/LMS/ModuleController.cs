using Femora.Application.Features.LMS.Modules.Commands.CreateModule;
using Femora.Application.Features.LMS.Modules.Commands.DeleteModule;
using Femora.Application.Features.LMS.Modules.Commands.ReorderModule;
using Femora.Application.Features.LMS.Modules.Commands.UpdateModule;
using Femora.Application.Features.LMS.Modules.Queries.GetModules;
using Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByCourse;
using Femora.Application.Features.LMS.Modules.Queries.ReadModule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Femora.API.Controllers.LMS;

/// <summary>
/// Module Management API Controller
/// Handles all module-related operations (CRUD, reordering, retrieval)
/// </summary>
[ApiController]
[Route("api/modules")]
[Authorize]
public class ModuleController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModuleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all modules for a specific course
    /// </summary>
    /// <param name="courseId">The course ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules for the course</returns>
    /// <response code="200">Returns list of modules</response>
    /// <response code="404">Course not found</response>
    [HttpGet("course/{courseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCourse(
        [FromRoute] Guid courseId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetModulesByCourseQuery { CourseId = courseId },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get a specific module by ID with all lessons and quiz details
    /// </summary>
    /// <param name="moduleId">The module ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Module details</returns>
    /// <response code="200">Returns module details</response>
    /// <response code="404">Module not found</response>
    [HttpGet("{moduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid moduleId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ReadModuleQuery(moduleId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new module in a course (Instructor only)
    /// </summary>
    /// <param name="command">Create module command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created module ID</returns>
    /// <response code="201">Module created successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Course not found</response>
    [HttpPost]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateModuleCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var moduleId = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { moduleId },
            new { id = moduleId });
    }

    /// <summary>
    /// Update an existing module (Instructor only)
    /// </summary>
    /// <param name="moduleId">The module ID</param>
    /// <param name="command">Update module command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Module updated successfully</response>
    /// <response code="400">Invalid input or ID mismatch</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Module not found</response>
    [HttpPut("{moduleId:guid}")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid moduleId,
        [FromBody] UpdateModuleCommand command,
        CancellationToken cancellationToken)
    {
        if (moduleId != command.Id)
            return BadRequest(new { message = "Module ID mismatch" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete a module (Instructor only)
    /// </summary>
    /// <param name="moduleId">The module ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Module deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Module not found</response>
    [HttpDelete("{moduleId:guid}")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid moduleId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteModuleCommand(moduleId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reorder modules within a course (Instructor only)
    /// </summary>
    /// <param name="command">Reorder modules command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Modules reordered successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    [HttpPut("reorder")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reorder(
        [FromBody] ReorderModuleCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
