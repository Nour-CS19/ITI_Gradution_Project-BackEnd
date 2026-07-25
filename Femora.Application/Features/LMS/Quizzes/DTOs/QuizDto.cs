namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ModuleId { get; set; }
    public int MinimumPassingScore { get; set; }
    public int MaxAttempts { get; set; }
}
