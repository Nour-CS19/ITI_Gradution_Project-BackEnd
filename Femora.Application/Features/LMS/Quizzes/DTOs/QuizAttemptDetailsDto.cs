namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizAttemptDetailsDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public decimal Score { get; set; }
    public int MaxScore { get; set; }
    public bool IsPassed { get; set; }

    public List<QuizAttemptAnswerDto> Answers { get; set; } = new();
}