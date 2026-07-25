using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.Register;

public sealed record RegisterCommand : IRequest<SigninResponseDto>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public Guid OnboardingGoalId { get; init; }
    public List<Guid> InterestIds { get; init; } = new();
}
