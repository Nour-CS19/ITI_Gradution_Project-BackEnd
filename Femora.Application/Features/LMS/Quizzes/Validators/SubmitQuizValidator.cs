using Femora.Application.Features.LMS.Quizzes.Commands.SubmitQuiz;
using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class SubmitQuizValidator : AbstractValidator<SubmitQuizCommand>
{
    public SubmitQuizValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();
        RuleFor(x => x.EnrollmentId).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty();
    }
}