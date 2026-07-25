using Femora.Application.Features.LMS.Lesson.DTOs;
using Femora.Application.Features.LMS.Quizzes.DTOs;

namespace Femora.Application.Features.LMS.Modules.DTOs;

public class ModuleDetailsDto
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int OrderIndex { get; set; }

    public List<LessonDto> Lessons { get; set; } = new();

    public QuizSummaryDto? Quiz { get; set; }
}
