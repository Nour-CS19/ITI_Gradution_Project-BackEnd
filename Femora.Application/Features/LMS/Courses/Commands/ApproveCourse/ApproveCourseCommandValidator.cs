using FluentValidation;

namespace Femora.Application.Features.LMS.Courses.Commands.ApproveCourse;

public class ApproveCourseCommandValidator : AbstractValidator<ApproveCourseCommand>
{
    public ApproveCourseCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AdminId).NotEmpty();
    }
}
