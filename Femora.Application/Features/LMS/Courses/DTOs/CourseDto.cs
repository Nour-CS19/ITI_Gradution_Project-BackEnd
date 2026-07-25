namespace Femora.Application.Features.LMS.Courses.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string Status { get; set; } = string.Empty;

    public string InstructorName { get; set; } = string.Empty;

    public int EnrollmentsCount { get; set; }
}