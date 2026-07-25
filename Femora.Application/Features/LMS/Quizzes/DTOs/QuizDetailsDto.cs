namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int MinimumPassingScore { get; set; }
    public int MaxAttempts { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}
