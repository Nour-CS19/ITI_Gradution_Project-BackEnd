using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.LMS;

namespace Femora.Application.Common.Interfaces.Repositories.LMS
{
    public interface IEnrollmentRepository
    {
        Task<bool> IsEnrolledAsync(Guid traineeProfileId, Guid courseId, CancellationToken ct = default);
        Task<Enrollment> EnrollAsync(Guid traineeProfileId, Guid courseId, decimal pricePaid, CancellationToken ct = default);

        // Lightweight projection used for "My Courses" listing: computes lesson/progress
        // counts with SQL COUNT(...) instead of loading the full Course/Modules/Lessons/
        // LessonProgresses entity graph into memory (which was the cause of the slow,
        // heavy "دوراتي" page load).
        IQueryable<EnrollmentDTO> GetMyEnrollmentsProjected(Guid traineeProfileId);
        Task<Enrollment?> GetDetailsAsync(Guid enrollmentId, Guid userId, CancellationToken cancellation = default);

        // Returns the enrollment with all related details without filtering by user.
        Task<Enrollment?> GetByIdWithDetailsAsync(Guid enrollmentId, CancellationToken cancellation = default);
        Task<Enrollment?> GetDetailsByIdAsync(Guid enrollmentId, CancellationToken cancellation = default);
    }
}
