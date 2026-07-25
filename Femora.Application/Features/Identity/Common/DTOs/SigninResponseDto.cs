using Femora.Domain.Enums;

namespace Femora.Application.Features.Identity.Common.DTOs;

public record SigninResponseDto
{
    public bool RequiresProfileSelection { get; set; }
    public List<AvailableProfileDto> AvailableProfiles { get; set; } = [];
    public AuthResponseDto? Auth { get; set; }
}

public record AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public ProfileType? ActiveProfile { get; set; }
    public UserDTO User { get; set; } = null!;
};

public record UserDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

public record AvailableProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}