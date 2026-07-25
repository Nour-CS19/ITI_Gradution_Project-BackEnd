using Femora.Application.Features.LMS.Lesson.DTOs;

namespace Femora.Application.Features.LMS.Modules.DTOs;

public class ModuleDto
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public int LessonsCount { get; set; }
    public List<LessonDto> Lessons { get; set; } = new();
}