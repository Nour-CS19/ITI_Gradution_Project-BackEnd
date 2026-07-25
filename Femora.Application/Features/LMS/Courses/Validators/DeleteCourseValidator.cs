using FluentValidation;
using Femora.Application.Features.LMS.Courses.Commands;

namespace Femora.Application.Features.LMS.Courses.Validators;

public class DeleteCourseValidator : AbstractValidator<DeleteCourseCommand>
{
    public DeleteCourseValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}