using System;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;

namespace Femora.Domain.Entities.LMS.Quizzes;

public class QuizAttempt : BaseEntity
{
    public Guid QuizId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid TraineeProfileId { get; set; }
    public decimal Score { get; set; }
    public int MaxScore { get; set; }
    // Percentage = Score / MaxScore * 100, stored explicitly so callers never have to
    // guess which of Score/MaxScore is the "out of 100" figure. Score/MaxScore stay as
    // "correct answers out of total questions" (e.g. 7/10); Percentage is the 0-100 value
    // used for pass/fail comparisons and for display like "70%".
    public decimal Percentage { get; set; }
    public bool IsPassed { get; set; } = false;
    public int AttemptNumber { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public Enrollment Enrollment { get; set; }
    public TraineeProfile TraineeProfile { get; set; }
    public Quiz Quiz { get; set; }
    // Answers for this attempt
    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}
