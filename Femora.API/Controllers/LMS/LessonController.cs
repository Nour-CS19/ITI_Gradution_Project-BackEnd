using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.LMS.Commands.UploadLessonResource;
using Femora.Application.Features.LMS.Lesson.Commands;
using Femora.Application.Features.LMS.Lesson.Queries;
using Femora.Application.Features.LMS.Lesson.Queries.GetLessonById;
using Femora.Application.Features.LMS.Lesson.Queries.GetLessonResources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.LMS;

/// <summary>
/// Lesson Management API Controller
/// Handles lesson CRUD operations and resource management
/// </summary>
[Route("api/lessons")]
[ApiController]
[Authorize]
public class LessonController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all lessons for a specific module
    /// </summary>
    /// <param name="moduleId">The module ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of lessons</returns>
    /// <response code="200">Returns list of lessons ordered by index</response>
    /// <response code="404">Module not found</response>
    [HttpGet("module/{moduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetByModule(
        [FromRoute] Guid moduleId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetLessonsByModuleQuery(moduleId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new lesson in a module (Instructor only)
    /// </summary>
    /// <param name="command">Create lesson command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created lesson ID</returns>
    /// <response code="201">Lesson created successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Module not found</response>
    [HttpPost]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLessonCommand command,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var lessonId = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { lessonId },
            new { id = lessonId });
    }

    /// <summary>
    /// Get a specific lesson by ID
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lesson details</returns>
    /// <response code="200">Returns lesson details</response>
    /// <response code="404">Lesson not found</response>
    [HttpGet("{lessonId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLessonByIdQuery(lessonId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing lesson (Instructor only)
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="command">Update lesson command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Lesson updated successfully</response>
    /// <response code="400">Invalid input or ID mismatch</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Lesson not found</response>
    [HttpPut("{lessonId:guid}")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid lessonId,
        [FromBody] UpdateLessonCommand command,
        CancellationToken cancellationToken)
    {
        if (lessonId != command.LessonId)
            return BadRequest(new { message = "Lesson ID mismatch" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete a lesson (Instructor only)
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Lesson deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Lesson not found</response>
    [HttpDelete("{lessonId:guid}")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid lessonId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new DeleteLessonCommand(lessonId),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Reorder lessons within a module (Instructor only)
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="command">Reorder lesson command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Lesson reordered successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    [HttpPut("{lessonId:guid}/reorder")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Reorder(
        [FromRoute] Guid lessonId,
        [FromBody] ReorderLessonCommand command,
        CancellationToken cancellationToken)
    {
        if (lessonId != command.LessonId)
            return BadRequest(new { message = "Lesson ID mismatch" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Upload a PDF/document resource for a lesson.
    /// The file is stored in Azure Blob Storage then automatically
    /// extracted → chunked → embedded → indexed in Azure AI Search (RAG pipeline).
    /// </summary>
    /// <param name="lessonId">The lesson ID</param>
    /// <param name="file">The file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with resource info</returns>
    /// <response code="200">File uploaded and indexed successfully</response>
    /// <response code="400">No file provided or invalid file</response>
    /// <response code="404">Lesson not found</response>
    [HttpPost("{lessonId:guid}/resources")]
    [Authorize(Policy = "Instructor")]
    [RequestSizeLimit(1_073_741_824)] // 1 GB - default ASP.NET Core limit (~28.6MB) would silently reject most lesson videos before they ever reach the indexing pipeline
    [RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)]
    [ProducesResponseType(typeof(UploadLessonResourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadResource(
        [FromRoute] Guid lessonId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file was provided." });

        var command = new UploadLessonResourceCommand
        {
            LessonId = lessonId,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists this lesson's uploaded resources (video/PDF/etc.) with their indexing
    /// status and, if failed, the exact error - so instructors can see WHY a
    /// video/document isn't showing up in summarize/chat instead of guessing.
    /// </summary>
    [HttpGet("{lessonId:guid}/resources")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResources(
        [FromRoute] Guid lessonId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLessonResourcesQuery(lessonId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Re-index an existing lesson resource.
    /// Deletes old chunks from Azure AI Search and re-runs the full RAG pipeline.
    /// </summary>
    /// <param name="lessonResourceId">The lesson resource ID</param>
    /// <param name="lessonIndexingRepository">Injected repository service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Resource re-indexed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Instructor role required</response>
    /// <response code="404">Resource not found</response>
    [HttpPost("resources/{lessonResourceId:guid}/reindex")]
    [Authorize(Policy = "Instructor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReindexResource(
        [FromRoute] Guid lessonResourceId,
        [FromServices] ILessonIndexingRepository lessonIndexingRepository,
        CancellationToken cancellationToken)
    {
        await lessonIndexingRepository.ReindexLessonResourceAsync(lessonResourceId, cancellationToken);
        return NoContent();
    }
}

