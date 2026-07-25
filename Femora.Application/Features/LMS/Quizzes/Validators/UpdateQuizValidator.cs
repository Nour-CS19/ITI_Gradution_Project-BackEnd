using FluentValidation;
using Femora.Application.Features.LMS.Quizzes.Commands;

namespace Femora.Application.Features.LMS.Quizzes.Validators;

public class UpdateQuizValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MinimumPassingScore)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0);
    }
}
