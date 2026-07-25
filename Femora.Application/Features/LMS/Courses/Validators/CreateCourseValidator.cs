using FluentValidation;
using Femora.Application.Features.LMS.Courses.Commands;

namespace Femora.Application.Features.LMS.Courses.Validators;

public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Language)
            .NotEmpty();

        RuleFor(x => x.Level)
            .IsInEnum();
    }
}