namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class SubmitQuizResultDto
{
    public Guid QuizAttemptId { get; set; }
    public decimal Score { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool IsPassed { get; set; }
    public int AttemptNumber { get; set; }
    public int MaxAttempts { get; set; }

    /// <summary>Regular + granted attempts still available after this submission (0 if none left).</summary>
    public int RemainingAttempts { get; set; }

    /// <summary>
    /// True when the trainee just used their last available attempt, failed, and hasn't
    /// already claimed the one-time AI weak-points bonus attempt for this quiz.
    /// The frontend uses this to show the "review your weak points" call to action.
    /// </summary>
    public bool CanRequestWeakPointsReview { get; set; }
}
