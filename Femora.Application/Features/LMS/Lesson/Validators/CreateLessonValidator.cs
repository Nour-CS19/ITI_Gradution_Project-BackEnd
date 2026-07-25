using FluentValidation;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public class CreateLessonValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonValidator()
    {
        RuleFor(x => x.ModuleId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .When(x => !string.IsNullOrEmpty(x.ContentUrl));

        RuleFor(x => x.OrderIndex)
            .GreaterThan(0);
    }
}