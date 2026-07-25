namespace Femora.Application.Features.LMS.Quizzes.DTOs;

public class QuizAttemptAnswerDto
{
    public Guid QuestionId { get; set; }
    public Guid ChoiceId { get; set; }
    public bool IsCorrect { get; set; }
}