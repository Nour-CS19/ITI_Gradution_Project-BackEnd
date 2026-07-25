using MediatR;

namespace Femora.Application.Features.Identity.Commands.ResetPassword;

/// <summary>Resets a user's password using the token sent via the forgot-password email link.</summary>
public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest;
