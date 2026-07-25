using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Femora.Application.Features.ProfileApplications.Commands.Approve;
using Femora.Application.Features.ProfileApplications.Commands.Cancel;
using Femora.Application.Features.ProfileApplications.Commands.Reject;
using Femora.Application.Features.ProfileApplications.Commands.Submit;
using Femora.Application.Features.ProfileApplications.Queries.GetList;
using Femora.Application.Features.ProfileApplications.Queries.GetMy;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Femora.API.Controllers.Identity;

[Route("api/profile-applications")]
[ApiController]
public class ProfileApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileApplicationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitProfileApplicationCommand command)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        command.UserId = userId;
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { id = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("my")]
    [Authorize]
    public async Task<IActionResult> Cancel()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var command = new CancelProfileApplicationCommand { UserId = userId };
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMy()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var query = new GetMyProfileApplicationQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("/api/admin/profile-applications")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetList(
        [FromQuery] ApplicationRequestStatus? status,
        [FromQuery] RequestedRole? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetProfileApplicationsQuery
        {
            Status = status,
            RequestedRole = role,
            PageNumber = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("/api/admin/profile-applications/{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var command = new ApproveProfileApplicationCommand
        {
            Id = id,
            AdminUserId = adminId
        };

        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("/api/admin/profile-applications/{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequestModel model)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var command = new RejectProfileApplicationCommand
        {
            Id = id,
            AdminUserId = adminId,
            RejectionReason = model.RejectionReason
        };

        try
        {
            await _mediator.Send(command);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class RejectRequestModel
{
    public string RejectionReason { get; set; } = string.Empty;
}
