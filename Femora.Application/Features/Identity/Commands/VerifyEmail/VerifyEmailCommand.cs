using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.VerifyEmail;

/// <summary>Confirms a user's email using the token sent via email link.</summary>
public sealed record VerifyEmailCommand(
    string UserId,
    string Token) : IRequest<SigninResponseDto>;
