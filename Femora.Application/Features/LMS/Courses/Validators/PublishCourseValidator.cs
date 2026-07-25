using FluentValidation;
using Femora.Application.Features.LMS.Courses.Commands;

namespace Femora.Application.Features.LMS.Courses.Validators;

public class PublishCourseValidator : AbstractValidator<PublishCourseCommand>
{
    public PublishCourseValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}