using System;
using Femora.Domain.Common;

namespace Femora.Domain.Entities.LMS.Quizzes;

/// <summary>
/// A one-time bonus attempt unlocked for a trainee after they exhaust every regular
/// attempt (Quiz.MaxAttempts) on a quiz and read the AI-generated "weak points" review
/// of their last attempt. Effectively extends the attempt cap by 1 without touching
/// Quiz.MaxAttempts (which stays the same for every other trainee).
/// </summary>
public class QuizRetryGrant : BaseEntity
{
    public Guid QuizId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid TraineeProfileId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    public Quiz? Quiz { get; set; }
}
