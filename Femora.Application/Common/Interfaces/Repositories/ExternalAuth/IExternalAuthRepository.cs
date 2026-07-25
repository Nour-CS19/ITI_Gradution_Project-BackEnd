namespace Femora.Application.Common.Interfaces.Repositories.ExternalAuth;

public record ExternalUserInfo(
    string ProviderKey,
    string Email,
    string FirstName,
    string LastName,
    string? PictureUrl);

public interface IExternalAuthRepository
{
    /// <summary>
    /// Validates an id_token (Google) or access_token (Facebook)
    /// and returns verified user info.
    /// Provider: "Google" | "Facebook"
    /// </summary>
    Task<ExternalUserInfo> ValidateTokenAsync(string provider, string token, CancellationToken cancellationToken = default);
}
