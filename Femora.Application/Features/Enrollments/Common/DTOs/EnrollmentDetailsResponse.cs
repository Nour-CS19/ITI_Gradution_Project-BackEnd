namespace Femora.Application.Features.Enrollments.Common.DTOs;

public class EnrollmentDetailsResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<ModuleDetailsDto> Modules { get; set; } = new List<ModuleDetailsDto>();
}

public class ModuleDetailsDto
{
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsUnlocked { get; set; }
    public bool AllLessonsCompleted { get; set; }
    public bool IsCompleted { get; set; }
    public bool QuizPassed { get; set; }

    public ICollection<LessonDetailsDto> Lessons { get; set; } = new List<LessonDetailsDto>();
}

// Metadata only - deliberately excludes ContentUrl/ContentMimeType/ContentText.
// Rendering a whole course (every module, every lesson) used to eagerly generate a
// blob SAS URL and load full article text for EVERY lesson on a single page load,
// even for locked modules the trainee can't open yet. The actual playable content
// (SAS video/PDF URL, article text) is now fetched lazily, one lesson at a time,
// via GET /api/lessons/{id} the moment the trainee actually opens that lesson.
public class LessonDetailsDto
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public int WatchedSeconds { get; set; }
    public string? ContentType { get; set; }
}
