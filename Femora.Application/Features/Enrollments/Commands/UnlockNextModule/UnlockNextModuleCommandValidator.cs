using FluentValidation;

namespace Femora.Application.Features.Enrollments.Commands.UnlockNextModule;

public class UnlockNextModuleCommandValidator :AbstractValidator<UnlockNextModuleCommand>
{
    public UnlockNextModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
        .NotEmpty()
        .WithMessage("Module ID is required.");
    }
}
