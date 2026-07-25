using FluentValidation;

namespace Femora.Application.Features.Approvals.Commands.ApplyInstructor;

public class ApplyInstructorCommandValidator : AbstractValidator<ApplyInstructorCommand>
{
    public ApplyInstructorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Bio)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.PortfolioUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.PortfolioUrl))
            .WithMessage("PortfolioUrl must be a valid URL");
    }
}