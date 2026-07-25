using FluentValidation;

namespace Femora.Application.Features.Ai.Commands.ChatWithLesson;

public class ChatWithLessonCommandValidator : AbstractValidator<ChatWithLessonCommand>
{
    public ChatWithLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("معرّف الدرس مطلوب");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("السؤال مطلوب")
            .MaximumLength(2000).WithMessage("السؤال طويل جدًا (الحد الأقصى 2000 حرف)");
    }
}
