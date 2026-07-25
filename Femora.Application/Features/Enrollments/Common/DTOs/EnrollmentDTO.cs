namespace Femora.Application.Features.Enrollments.Common.DTOs;

public class EnrollmentDTO
{
    public Guid EnrollmentId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public decimal PricePaid { get; init; }
    public DateTime EnrolledAt { get; init; }
    public bool IsCompleted { get; init; }
    public int TotalLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int ProgressPercent { get; init; }
}
