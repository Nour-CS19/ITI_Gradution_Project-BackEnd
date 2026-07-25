using Femora.Application.Common.Interfaces.Repositories.ExternalAuth;
using Femora.Infrastructure.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Femora.Infrastructure.Repositories.ExternalAuth;

public class ExternalAuthRepository(
    IHttpClientFactory httpClientFactory,
    IOptions<ExternalAuthOptions> options,
    ILogger<ExternalAuthRepository> logger) : IExternalAuthRepository
{
    private readonly ExternalAuthOptions _opts = options.Value;

    public async Task<ExternalUserInfo> ValidateTokenAsync(
        string provider,
        string token,
        CancellationToken cancellationToken = default)
    {
        return provider.ToUpperInvariant() switch
        {
            "GOOGLE"   => await ValidateGoogleAsync(token, cancellationToken),
            "FACEBOOK" => await ValidateFacebookAsync(token, cancellationToken),
            _          => throw new NotSupportedException($"Provider '{provider}' is not supported.")
        };
    }


    private async Task<ExternalUserInfo> ValidateGoogleAsync(string idToken, CancellationToken ct)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_opts.Google.ClientId]
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new ExternalUserInfo(
                ProviderKey: payload.Subject,
                Email:       payload.Email,
                FirstName:   payload.GivenName  ?? string.Empty,
                LastName:    payload.FamilyName  ?? string.Empty,
                PictureUrl:  payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google token validation failed");
            throw new UnauthorizedAccessException("Invalid Google token.", ex);
        }
    }


    private async Task<ExternalUserInfo> ValidateFacebookAsync(string accessToken, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Facebook");

        var appToken  = $"{_opts.Facebook.AppId}|{_opts.Facebook.AppSecret}";
        var debugUrl  = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appToken}";
        var debugResp = await client.GetFromJsonAsync<JsonElement>(debugUrl, ct);
        var data      = debugResp.GetProperty("data");
        var isValid   = data.TryGetProperty("is_valid", out var v) && v.GetBoolean();

        if (!isValid)
        {
            var error = data.TryGetProperty("error", out var e) ? e.GetRawText() : "unknown";
            logger.LogWarning("Facebook token invalid: {Error}", error);
            throw new UnauthorizedAccessException("Invalid Facebook access token.");
        }

        var userUrl  = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,name,picture.type(large)&access_token={accessToken}";
        var userResp = await client.GetFromJsonAsync<JsonElement>(userUrl, ct);

        var facebookId = userResp.GetProperty("id").GetString()!;

        string email;
        if (userResp.TryGetProperty("email", out var emailProp) &&
            !string.IsNullOrWhiteSpace(emailProp.GetString()))
        {
            email = emailProp.GetString()!;
        }
        else
        {
           
            email = $"fb_{facebookId}@femora.facebook.user";
            logger.LogInformation(
                "Facebook user {Id} has no email — using generated fallback: {Email}",
                facebookId, email);
        }

        // 4. Parse name fields
        var firstName = userResp.TryGetProperty("first_name", out var fn)
            ? fn.GetString() ?? string.Empty
            : string.Empty;

        var lastName = userResp.TryGetProperty("last_name", out var ln)
            ? ln.GetString() ?? string.Empty
            : string.Empty;

        // Fallback: use full name if first/last not available
        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
        {
            var fullName = userResp.TryGetProperty("name", out var nm)
                ? nm.GetString() ?? string.Empty
                : string.Empty;

            var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = parts.ElementAtOrDefault(0) ?? "Facebook";
            lastName  = parts.ElementAtOrDefault(1) ?? "User";
        }

        // 5. Profile picture
        var pictureUrl = userResp.TryGetProperty("picture", out var pic)
                      && pic.TryGetProperty("data", out var picData)
                      && picData.TryGetProperty("url", out var urlProp)
            ? urlProp.GetString()
            : null;

        return new ExternalUserInfo(
            ProviderKey: facebookId,
            Email:       email,
            FirstName:   firstName,
            LastName:    lastName,
            PictureUrl:  pictureUrl);
    }
}
