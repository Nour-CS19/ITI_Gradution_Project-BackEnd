namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizAttemptDto
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public decimal Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime AttemptedAt { get; set; }
}