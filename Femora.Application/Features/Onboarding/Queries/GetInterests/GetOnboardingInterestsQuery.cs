using MediatR;

namespace Femora.Application.Features.Onboarding.Queries.GetInterests
{
    public record GetOnboardingInterestsQuery : IRequest<List<OnboardingInterestDto>>
    {
    }
}
