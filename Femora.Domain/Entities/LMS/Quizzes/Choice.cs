using System;
using Femora.Domain.Common;

namespace Femora.Domain.Entities.LMS.Quizzes;

public class Choice : BaseEntity
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsCorrect { get; set; } = false;

    // Navigation
    public Question? Question { get; set; }
}
