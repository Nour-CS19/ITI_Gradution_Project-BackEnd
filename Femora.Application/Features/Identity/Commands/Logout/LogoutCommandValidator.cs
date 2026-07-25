using FluentValidation;

namespace Femora.Application.Features.Identity.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .MinimumLength(20).WithMessage("Invalid refresh token format")
            .When(x => !string.IsNullOrEmpty(x.RefreshToken));
    }
}
