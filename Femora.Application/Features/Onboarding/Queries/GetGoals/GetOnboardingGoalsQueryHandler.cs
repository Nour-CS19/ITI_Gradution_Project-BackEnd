using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Onboarding.Queries.GetGoals
{
    public class GetOnboardingGoalsQueryHandler(IAppDbContext db)
        : IRequestHandler<GetOnboardingGoalsQuery, List<OnboardingGoalDto>>
    {
        public async Task<List<OnboardingGoalDto>> Handle(
            GetOnboardingGoalsQuery request,
            CancellationToken cancellationToken)
        {
            var goals = await db.OnboardingGoals
                .AsNoTracking()
                .Where(g => g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .Select(g => new OnboardingGoalDto
                {
                    Id = g.Id,
                    LabelAr = g.LabelAr,
                    LabelEn = g.LabelEn,
                    DescriptionAr = g.DescriptionAr,
                    DescriptionEn = g.DescriptionEn,
                    Emoji = g.Emoji,
                    DisplayOrder = g.DisplayOrder,
                    IsActive = g.IsActive
                })
                .ToListAsync(cancellationToken);

            return goals;
        }
    }
}
