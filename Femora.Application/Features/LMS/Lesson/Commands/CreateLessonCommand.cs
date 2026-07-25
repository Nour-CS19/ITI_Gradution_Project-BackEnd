using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public record CreateLessonCommand(
    Guid ModuleId,
    string Title,
    string? ArticleContent,
    string? ContentUrl,
    int DurationSeconds,
    int OrderIndex,
    bool IsPreview
) : IRequest<Guid>;