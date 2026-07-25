using Femora.Application.Features.Identity.Common.Exceptions;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Femora.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> _userManager,
    ILogger<ResetPasswordCommandHandler> _logger)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidTokenException("رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.");

        // Decode the Base64URL-encoded token back to the original token.
        var decodedToken = Base64UrlDecode(request.Token);

        _logger.LogDebug("Attempting to reset password for user {UserId}. Token decoded length: {TokenLength}", user.Id, decodedToken?.Length ?? 0);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}. Errors: {Errors}", user.Id, errors);
            throw new InvalidTokenException($"رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية. {errors}");
        }

        _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);
    }

    private static string Base64UrlDecode(string input)
    {
        // Add padding if needed
        var padding = (4 - (input.Length % 4)) % 4;
        var paddedInput = input + new string('=', padding);

        // Convert from base64url: replace -, _ back to +, /
        var base64 = paddedInput
            .Replace("-", "+")
            .Replace("_", "/");

        try
        {
            var bytes = Convert.FromBase64String(base64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            throw new InvalidTokenException("رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.");
        }
    }
}

