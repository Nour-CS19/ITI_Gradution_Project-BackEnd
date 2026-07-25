namespace Femora.Application.Features.Enrollments.Common.DTOs;

public record IsEnrolledResponse
{
    public bool IsEnrolled { get; init; }
    public Guid? EnrollmentId { get; init; }
    public bool IsCompleted { get; init; }
}
