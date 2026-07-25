using MediatR;

namespace Femora.Application.Features.Ai.Commands.GenerateLessonKeyQuestionsPdf;

public record GenerateLessonKeyQuestionsPdfCommand : IRequest<GenerateLessonKeyQuestionsPdfResponse>
{
    public Guid LessonId { get; init; }

    /// <summary>How many Q&amp;A pairs to include - default is enough for a quick review sheet.</summary>
    public int QuestionCount { get; init; } = 8;
}

public record GenerateLessonKeyQuestionsPdfResponse
{
    public Guid LessonId { get; init; }
    public string LessonTitle { get; init; } = string.Empty;
    public string PdfUrl { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
}
