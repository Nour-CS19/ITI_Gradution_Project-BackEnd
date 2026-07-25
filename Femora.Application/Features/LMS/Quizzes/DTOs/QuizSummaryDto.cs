namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionsCount { get; set; }
    public int MaxAttempts { get; set; }
}
