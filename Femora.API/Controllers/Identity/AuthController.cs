using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Settings;
using Femora.Application.Features.Identity.Commands.ExternalLogin;
using Femora.Application.Features.Identity.Commands.ForgotPassword;
using Femora.Application.Features.Identity.Commands.Login;
using Femora.Application.Features.Identity.Commands.Logout;
using Femora.Application.Features.Identity.Commands.Profile;
using Femora.Application.Features.Identity.Commands.RefreshToken;
using Femora.Application.Features.Identity.Commands.Register;
using Femora.Application.Features.Identity.Commands.ResetPassword;
using Femora.Application.Features.Identity.Commands.SendOtp;
using Femora.Application.Features.Identity.Commands.VerifyOtp;
using Femora.Application.Features.Identity.Common.Exceptions;
using Femora.Application.Common.Exceptions;
using Femora.Domain.Entities;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Femora.Application.Features.Identity.Common.DTOs;

namespace Femora.API.Controllers.Identity;

[Route("api/auth")]
[ApiController]
public class AuthController(
    IMediator mediator,
    IAuthService authService,
    IOptions<JwtSettings> jwtSettingsOptions,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        SetRefreshTokenInCookie(result.Auth!.RefreshToken);

        // After registration, send OTP for email verification
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is not null && !user.EmailConfirmed)
            await mediator.Send(new SendOtpCommand(command.Email), cancellationToken);

        return Ok(new
        {
            requiresProfileSelection  = false,
            emailVerificationRequired = true,
            auth = new
            {
                result.Auth.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> Signin([FromBody] SigninCommand command, CancellationToken cancellationToken)
    {
        SigninResponseDto result;
        try
        {
            result = await mediator.Send(command, cancellationToken);
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(new { title = "بيانات الدخول غير صحيحة", detail = ex.Message });
        }

        if (result.RequiresProfileSelection)
        {
            SetRefreshTokenInCookie(result.Auth!.RefreshToken);
            return Ok(new
            {
                requiresProfileSelection = true,
                availableProfiles = result.AvailableProfiles,
                auth = new
                {
                    result.Auth.AccessToken,
                    result.Auth.User,
                    activeProfile = (object?)null
                }
            });
        }

        SetRefreshTokenInCookie(result.Auth!.RefreshToken);
        return Ok(new
        {
            requiresProfileSelection = false,
            auth = new
            {
                result.Auth.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }

    [HttpPost("select-profile")]
    [Authorize]
    public async Task<IActionResult> SelectProfile([FromBody] SelectProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        SetRefreshTokenInCookie(result.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = false,
            auth = new
            {
                result.User,
                result.AccessToken,
                result.ActiveProfile,
            }
        });
    }

    [HttpPost("setup-profiles")]
    [Authorize]
    public async Task<IActionResult> SetupProfiles([FromBody] SetupProfilesCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (result.Auth is not null)
            SetRefreshTokenInCookie(result.Auth.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = result.RequiresProfileSelection,
            availableProfiles        = result.AvailableProfiles,
            pendingApproval          = result.Auth?.ActiveProfile is null && !result.RequiresProfileSelection,
            auth = result.Auth is null ? null : new
            {
                result.Auth.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }

    [HttpPost("/api/auth/logout")]
    [AllowAnonymous] // Allow even if token expired — just revoke cookie
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];

        // Always delete the cookie regardless of token validity
        DeleteRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
            return NoContent();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return NoContent(); // Guest or expired token — cookie already cleared

        try
        {
            await mediator.Send(new LogoutCommand(userId, refreshToken), cancellationToken);
        }
        catch (InvalidTokenException)
        {
            // Token already revoked or expired — treat as successful logout
        }
        catch (Exception)
        {
            // Never fail a logout — user is already signed out on the client
        }

        return NoContent();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken cancellation)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { title = "Invalid Token", detail = "Refresh token is missing or invalid" });

        var result = await mediator.Send(new RefreshTokenCommand(refreshToken), cancellation);
        SetRefreshTokenInCookie(result.Auth!.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = false,
            auth = new
            {
                result.Auth.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }


    [HttpPost("sync-profile")]
    [Authorize]
    public async Task<IActionResult> SyncProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { title = "Invalid Token", detail = "User id claim is missing or invalid" });

        var result = await authService.SelectProfileAsync(userId, ProfileType.Trainee);
        SetRefreshTokenInCookie(result.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = false,
            auth = new
            {
                result.User,
                result.AccessToken,
                result.ActiveProfile,
            }
        });
    }

    /// <summary>Send a 6-digit OTP to the user's email for verification.</summary>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok(new { message = "تم إرسال كود التحقق إلى بريدك الإلكتروني." });
    }

    /// <summary>Verify the 6-digit OTP and confirm the user's email.</summary>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        SetRefreshTokenInCookie(result.Auth!.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = false,
            auth = new
            {
                result.Auth!.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }

    /// <summary>Sends a password-reset link to the given email if an account exists (always returns success to avoid leaking which emails are registered).</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok(new { message = "إذا كان هذا البريد الإلكتروني مسجلاً لدينا، فسيصلك رابط إعادة تعيين كلمة المرور." });
    }

    /// <summary>Resets the password using the token sent via the forgot-password email link.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok(new { message = "تم تغيير كلمة المرور بنجاح. يمكنكِ الآن تسجيل الدخول." });
    }

    [HttpPost("external-login")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin(
        [FromBody] ExternalLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        SetRefreshTokenInCookie(result.Auth!.RefreshToken);

        return Ok(new
        {
            requiresProfileSelection = result.RequiresProfileSelection,
            availableProfiles = result.AvailableProfiles,
            auth = new
            {
                result.Auth.User,
                result.Auth.AccessToken,
                result.Auth.ActiveProfile,
            }
        });
    }

    private void SetRefreshTokenInCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires  = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax
        });
    }
}
