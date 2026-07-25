using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Onboarding;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Onboarding.Queries.GetInterests
{
    public class GetOnboardingInterestsQueryHandler(IAppDbContext db)
        : IRequestHandler<GetOnboardingInterestsQuery, List<OnboardingInterestDto>>
    {
        public async Task<List<OnboardingInterestDto>> Handle(
            GetOnboardingInterestsQuery request,
            CancellationToken cancellationToken)
        {
            var interests = await db.OnboardingInterests
                .AsNoTracking()
                .Where(i => i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new OnboardingInterestDto
                {
                    Id = i.Id,
                    NameAr = i.NameAr,
                    NameEn = i.NameEn,
                    DescriptionAr = i.DescriptionAr,
                    DescriptionEn = i.DescriptionEn,
                    DisplayOrder = i.DisplayOrder,
                    IsActive = i.IsActive
                })
                .ToListAsync(cancellationToken);

            return interests;
        }
    }
}
