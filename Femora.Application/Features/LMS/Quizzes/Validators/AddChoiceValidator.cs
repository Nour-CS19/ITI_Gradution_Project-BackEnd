using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class AddChoiceValidator : AbstractValidator<AddChoiceCommand>
{
    public AddChoiceValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(300);
    }
}