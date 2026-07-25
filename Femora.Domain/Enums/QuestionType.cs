namespace Femora.Domain.Enums;

/// <summary>
/// Distinguishes standard multiple-choice questions from True/False questions,
/// so the API and the trainee-facing UI know how to render each question and
/// so the AI generator/validators can enforce the right shape (e.g. exactly
/// 2 choices for TrueFalse).
/// </summary>
public enum QuestionType
{
    MultipleChoice = 0,
    TrueFalse = 1
}
