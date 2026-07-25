using Femora.Application.Features.Admin.Commands.SetUserActive;
using Femora.Application.Features.Admin.Commands.ResetAndReindexVideoLessons;
using Femora.Application.Features.Admin.Queries.GetAdminStats;
using Femora.Application.Features.Admin.Queries.GetAllUsers;
using Femora.Application.Features.Admin.Queries.GetAllOrders;
using Femora.Application.Features.Admin.Queries.GetAllProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.Admin;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns platform-wide stats for the admin dashboard:
    /// user counts, course/product counts, total revenue, pending approvals.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] GetAllUsersQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] GetAllOrdersQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] GetAllProductsQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPatch("users/{userId:guid}/active")]
    public async Task<IActionResult> SetUserActive(Guid userId, [FromBody] bool isActive, CancellationToken ct)
    {
        await mediator.Send(new SetUserActiveCommand { UserId = userId, IsActive = isActive }, ct);
        return NoContent();
    }

    /// <summary>
    /// Recovery for "Storage quota has been exceeded" from Azure Search (free/basic tier
    /// 50MB cap): wipes the lesson-chunks index entirely, then re-indexes every video
    /// lesson currently in the DB from scratch. Safe to call any time storage-quota
    /// errors show up in logs, or after resetting/reseeding the database during dev.
    /// May take a while for many lessons (300ms delay between each to avoid re-tripping
    /// embedding rate limits) - the response reports how many succeeded vs failed.
    /// </summary>
    [HttpPost("search-index/reset-and-reindex-videos")]
    public async Task<IActionResult> ResetAndReindexVideoLessons(CancellationToken ct)
    {
        var result = await mediator.Send(new ResetAndReindexVideoLessonsCommand(), ct);
        return Ok(result);
    }
}