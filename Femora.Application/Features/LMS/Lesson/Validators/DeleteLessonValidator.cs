using Femora.Application.Features.LMS.Lesson.Commands;
using FluentValidation;

namespace Femora.Application.Features.LMS.Lesson.Validators;

public class DeleteLessonValidator : AbstractValidator<DeleteLessonCommand>
{
    public DeleteLessonValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
    }
}