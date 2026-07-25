using FluentValidation;

namespace Femora.Application.Features.LMS.Quizzes.Commands.SubmitQuiz;

public class SubmitQuizCommandValidator : AbstractValidator<SubmitQuizCommand>
{
    public SubmitQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("QuizId is required");

        RuleFor(x => x.TraineeProfileId)
            .NotEmpty().WithMessage("TraineeProfileId is required");

        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("At least one answer is required");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId)
                .NotEmpty().WithMessage("QuestionId is required");

            answer.RuleFor(a => a.ChoiceId)
                .NotEmpty().WithMessage("ChoiceId is required");
        });
    }
}