using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Identity.Services;

public class OnboardingProfileSyncService(IAppDbContext _context) : IOnboardingProfileSyncService
{
    public async Task<TraineeProfileSyncResult> EnsureTraineeProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), userId.ToString());

        var traineeProfile = await _context.TraineeProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == userId, cancellationToken);

        var wasCreated = traineeProfile is null;

        if (traineeProfile is null)
        {
            traineeProfile = new TraineeProfile { UserId = userId };
            await _context.TraineeProfiles.AddAsync(traineeProfile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await SyncGoalAsync(user, traineeProfile, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new TraineeProfileSyncResult(traineeProfile.Id, wasCreated);
    }

    private async Task SyncGoalAsync(ApplicationUser user, TraineeProfile traineeProfile, CancellationToken cancellationToken)
    {
        if (!user.OnboardingGoalId.HasValue)
            return;

        var existingGoals = await _context.LearningGoals
            .Where(g => g.TraineeProfileId == traineeProfile.Id)
            .ToListAsync(cancellationToken);

        var currentGoal = existingGoals
            .FirstOrDefault(g => g.OnboardingGoalId == user.OnboardingGoalId.Value);

        if (currentGoal is null)
        {
            _context.LearningGoals.RemoveRange(existingGoals);
            _context.LearningGoals.Add(new TraineeLearningGoal
            {
                TraineeProfileId = traineeProfile.Id,
                OnboardingGoalId = user.OnboardingGoalId.Value
            });
        }
    }
}
