using Femora.Application.Features.Identity.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Femora.Application.Features.Identity.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<ProfileDto>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? GitHubUrl { get; init; }
    public string? Country { get; init; }

    /// <summary>Optional new avatar image. When omitted, the existing avatar is kept.</summary>
    public IFormFile? Avatar { get; init; }
}
