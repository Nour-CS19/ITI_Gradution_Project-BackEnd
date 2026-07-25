using Femora.Application.Common.Interfaces.Repositories.Email;
using Femora.Application.Common.Settings;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Web;

namespace Femora.Application.Features.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> _userManager,
    IEmailRepository _emailRepository,
    IOptions<ClientAppOptions> _clientAppOptions)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Don't reveal whether the email exists — always behave the same way from the caller's perspective.
        if (user is null || !user.IsActive)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Use Base64URL encoding to safely handle special characters in the token.
        // This avoids issues with +, /, and = characters that HttpUtility.UrlEncode doesn't handle well.
        var encodedToken = Base64UrlEncode(token);
        var encodedEmail = HttpUtility.UrlEncode(user.Email);
        var resetLink = $"{_clientAppOptions.Value.BaseUrl.TrimEnd('/')}/reset-password?token={encodedToken}&email={encodedEmail}";

        await _emailRepository.SendPasswordResetAsync(
            user.Email!,
            $"{user.FirstName} {user.LastName}",
            resetLink,
            cancellationToken);
    }

    private static string Base64UrlEncode(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        // Standard base64 encode
        var base64 = Convert.ToBase64String(inputBytes);
        // Convert to base64url: replace +, /, and remove padding
        return base64
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
