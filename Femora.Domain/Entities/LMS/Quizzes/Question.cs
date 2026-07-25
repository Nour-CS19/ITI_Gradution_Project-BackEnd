using System;
using System.Collections.Generic;
using Femora.Domain.Common;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS.Quizzes;

public class Question : BaseEntity
{
    public Guid QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    public ICollection<Choice> Choices { get; set; } = new List<Choice>();

    // Navigation
    public Quiz? Quiz { get; set; }
}
