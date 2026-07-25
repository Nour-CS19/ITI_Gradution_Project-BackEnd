using Femora.Domain.Common;

namespace Femora.Domain.Entities.LMS.Quizzes;

public class QuizAttemptAnswer : BaseEntity
{
    public Guid QuizAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid ChoiceId { get; set; }
    public bool IsCorrect { get; set; }
    public QuizAttempt QuizAttempt { get; set; }
    public Question Question { get; set; }
    public Choice Choice { get; set; }
}
