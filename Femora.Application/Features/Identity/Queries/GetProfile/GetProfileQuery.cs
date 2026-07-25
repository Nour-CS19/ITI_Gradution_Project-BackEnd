using MediatR;
using System;

namespace Femora.Application.Features.Identity.Queries.GetProfile;

public record GetProfileQuery : IRequest<ProfileDto>
{
    public Guid UserId { get; init; }
}

public record ProfileDto
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? GitHubUrl { get; init; }
    public string? Country { get; init; }
}
