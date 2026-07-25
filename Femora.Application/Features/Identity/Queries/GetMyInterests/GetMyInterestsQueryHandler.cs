using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Queries.GetMyInterests;

public class GetMyInterestsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetMyInterestsQuery, MyInterestsResponse>
{
    public async Task<MyInterestsResponse> Handle(GetMyInterestsQuery request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .AsNoTracking()
            .Include(u => u.OnboardingInterests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId.ToString());

        var selectedInterestIds = user.OnboardingInterests
            .Select(i => i.Id)
            .ToHashSet();

        var allInterests = await db.OnboardingInterests
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

        var mappedInterests = allInterests.Select(i => new UserInterestDto
        {
            Id = i.Id,
            NameAr = i.NameAr,
            NameEn = i.NameEn,
            DescriptionAr = i.DescriptionAr,
            DescriptionEn = i.DescriptionEn,
            IsSelected = selectedInterestIds.Contains(i.Id)
        }).ToList();

        return new MyInterestsResponse
        {
            Interests = mappedInterests
        };
    }
}
