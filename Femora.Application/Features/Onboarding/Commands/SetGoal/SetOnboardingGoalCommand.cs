using MediatR;

namespace Femora.Application.Features.Onboarding.Commands.SetGoal;

public record SetOnboardingGoalCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
    public Guid GoalId { get; init; }
}
