using FluentValidation;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public class UpdateLessonValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .When(x => !string.IsNullOrEmpty(x.ContentUrl));

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0);
    }
}