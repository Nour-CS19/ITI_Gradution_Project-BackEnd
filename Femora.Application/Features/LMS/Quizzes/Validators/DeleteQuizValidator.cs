using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class DeleteQuizValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}