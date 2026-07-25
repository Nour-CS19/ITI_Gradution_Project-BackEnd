using FluentValidation;

namespace Femora.Application.Features.Ai.Commands.RenameConversation;

public class RenameConversationCommandValidator : AbstractValidator<RenameConversationCommand>
{
    public RenameConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("العنوان مطلوب")
            .MaximumLength(200).WithMessage("العنوان طويل جدًا");
    }
}
