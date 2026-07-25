using Femora.Application.Features.LMS.Modules.DTOs;

namespace Femora.Application.Features.LMS.Courses.DTOs;

public class CourseDetailsDto
{
    public Guid Id { get; set; }

    public Guid InstructorProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string InstructorName { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int EnrollmentsCount { get; set; }

    public int TotalLessons { get; set; }

    public List<ModuleDto> Modules { get; set; } = new();
}