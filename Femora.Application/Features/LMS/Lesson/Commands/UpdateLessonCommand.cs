using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public record UpdateLessonCommand(
    Guid LessonId,
    string Title,
    string? ArticleContent,
    string? ContentUrl,
    int DurationSeconds,
    int OrderIndex,
    bool IsPreview
) : IRequest;