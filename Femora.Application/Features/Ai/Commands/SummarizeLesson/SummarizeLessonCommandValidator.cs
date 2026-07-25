using FluentValidation;
using System.Linq;

namespace Femora.Application.Features.Ai.Commands.SummarizeLesson;

public class SummarizeLessonCommandValidator : AbstractValidator<SummarizeLessonCommand>
{
    private static readonly string[] AllowedLengths = { "short", "medium", "detailed" };

    public SummarizeLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("معرّف الدرس مطلوب");

        RuleFor(x => x.Length)
            .Must(length => AllowedLengths.Contains(length.ToLowerInvariant()))
            .WithMessage("طول التلخيص يجب أن يكون short أو medium أو detailed");
    }
}
