using MediatR;

namespace Femora.Application.Features.Onboarding.Queries.GetGoals
{
    public record GetOnboardingGoalsQuery : IRequest<List<OnboardingGoalDto>>
    {
    }
}
