using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Onboarding;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.SetUserInterests;

public class SetUserInterestsCommandHandler(IAppDbContext db)
    : IRequestHandler<SetUserInterestsCommand, Unit>
{
    public async Task<Unit> Handle(SetUserInterestsCommand request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .Include(u => u.OnboardingInterests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId.ToString());

        var cleanIds = (request.InterestIds ?? Enumerable.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (cleanIds.Count > 0)
        {
            var interests = await db.OnboardingInterests
                .Where(i => cleanIds.Contains(i.Id) && i.IsActive)
                .ToListAsync(cancellationToken);

            if (interests.Count != cleanIds.Count)
                throw new NotFoundException("OnboardingInterest", "One or more selected interests were not found or are inactive.");

            user.OnboardingInterests.Clear();
            foreach (var interest in interests)
            {
                user.OnboardingInterests.Add(interest);
            }
        }
        else
        {
            user.OnboardingInterests.Clear();
        }

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
