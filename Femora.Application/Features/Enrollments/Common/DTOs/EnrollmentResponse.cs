using Femora.Domain.Enums;

namespace Femora.Application.Features.Enrollments.Common.DTOs;

public class EnrollmentResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public decimal PricePaid { get; set; }
    public Guid? FirstModuleId { get; set; }
    public EnrollmentStatus Status { get; set; }

    // Lazy trainee-profile activation info. Populated only the first time a
    // user enrolls in a course and didn't already have a Trainee profile —
    // it was just created for them automatically. The frontend should show
    // ActivationMessage to the user and, if AccessToken/RefreshToken are set,
    // silently swap them in so the newly-activated Trainee profile is usable
    // right away without a re-login.
    public bool TraineeProfileActivated { get; set; }
    public string? ActivationMessage { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
