using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities;
using Femora.Domain.Entities.LMS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Onboarding.Commands.SetGoal;

public class SetOnboardingGoalCommandHandler(IAppDbContext db)
    : IRequestHandler<SetOnboardingGoalCommand, Unit>
{
    public async Task<Unit> Handle(SetOnboardingGoalCommand request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId.ToString());

        var goalExists = await db.OnboardingGoals
            .AnyAsync(g => g.Id == request.GoalId && g.IsActive, cancellationToken);

        if (!goalExists)
            throw new NotFoundException("OnboardingGoal", request.GoalId.ToString());

        user.OnboardingGoalId = request.GoalId;

        var traineeProfile = await db.TraineeProfiles
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (traineeProfile is not null)
        {
            var existingGoals = await db.LearningGoals
                .Where(g => g.TraineeProfileId == traineeProfile.Id)
                .ToListAsync(cancellationToken);

            db.LearningGoals.RemoveRange(existingGoals);
            db.LearningGoals.Add(new TraineeLearningGoal
            {
                TraineeProfileId = traineeProfile.Id,
                OnboardingGoalId = request.GoalId
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
