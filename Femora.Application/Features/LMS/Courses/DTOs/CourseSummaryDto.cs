namespace Femora.Application.Features.LMS.Courses.DTOs;

public class CourseSummaryDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
}