using Femora.Application.Features.Identity.Commands.UpdateProfile;
using Femora.Application.Features.Identity.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.API.Controllers.Identity;

[Route("api/profile")]
[ApiController]
[Authorize]
public class ProfileController(IMediator mediator) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim not found."));

    /// <summary>
    /// Gets the current user's personal profile.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProfileQuery { UserId = CurrentUserId }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the current user's personal profile. Accepts multipart/form-data so an
    /// avatar image can be uploaded alongside the other fields.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand
        {
            UserId = CurrentUserId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Bio = request.Bio,
            LinkedInUrl = request.LinkedInUrl,
            GitHubUrl = request.GitHubUrl,
            Country = request.Country,
            Avatar = request.Avatar
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Country { get; set; }
    public IFormFile? Avatar { get; set; }
}
