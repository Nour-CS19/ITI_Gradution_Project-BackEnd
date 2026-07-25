using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class CreateQuizValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MinimumPassingScore)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0);
    }
}