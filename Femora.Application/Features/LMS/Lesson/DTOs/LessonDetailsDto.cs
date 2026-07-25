using Femora.Domain.Enums;

namespace Femora.Application.Features.LMS.Lesson.DTOs;

public class LessonDetailsDto
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }

    public string Title { get; set; } = string.Empty;
    public LessonType Type { get; set; }
    public string? ArticleContent { get; set; }
    public string? ContentUrl { get; set; }
    public string? ContentType { get; set; }
    public string? ContentMimeType { get; set; }
    public string? ContentText { get; set; }

    public int DurationSeconds { get; set; }
    public int OrderIndex { get; set; }

    public bool IsPreview { get; set; }
}
