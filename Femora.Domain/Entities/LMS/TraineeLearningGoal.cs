using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Onboarding;

namespace Femora.Domain.Entities.LMS;

public class TraineeLearningGoal : BaseEntity
{
    public Guid TraineeProfileId { get; set; }
    public Guid? OnboardingGoalId { get; set; }
    public OnboardingGoal? Goal { get; set; }
    public TraineeProfile TraineeProfile { get; set; } = null!;
}
