using Femora.Application.Features.LMS.Quizzes.Commands;
using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Validators;

public class AddQuestionValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
    }
}