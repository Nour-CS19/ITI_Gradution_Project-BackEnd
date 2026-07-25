namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizWeakPointsReportDto
{
    public Guid QuizId { get; set; }
    public Guid QuizAttemptId { get; set; }

    /// <summary>One entry per question the trainee got wrong on their last attempt.</summary>
    public List<QuizWeakPointItemDto> WeakPoints { get; set; } = new();

    /// <summary>One short, encouraging study tip covering all weak points together.</summary>
    public string OverallTip { get; set; } = string.Empty;

    /// <summary>True the first time this report is generated - a bonus attempt was just unlocked.</summary>
    public bool RetryUnlocked { get; set; }
}

public class QuizWeakPointItemDto
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string YourAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
