using MediatR;

namespace Femora.Application.Features.Identity.Commands.ForgotPassword;

/// <summary>Requests a password-reset link be emailed to the given address (if an account exists).</summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest;
