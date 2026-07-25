using Femora.Domain.Common;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Identity;
public class TraineeProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public TrainingSkillLevel SkillLevel { get; set; } = TrainingSkillLevel.Beginner;
    public ApplicationUser User { get; set; }
    public ICollection<TraineeLearningGoal> LearningGoals { get; set; } = new List<TraineeLearningGoal>();
    public ICollection<TraineePreferredCategory> PreferredCategories { get; set; } = new List<TraineePreferredCategory>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
