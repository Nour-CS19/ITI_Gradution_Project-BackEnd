using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Repositoies.LMS
{
    public class EnrollmentRepository(IAppDbContext context) : IEnrollmentRepository
    {
        public async Task<bool> IsEnrolledAsync(Guid traineeProfileId, Guid courseId, CancellationToken ct = default)
        {
            return await context.Enrollments
                .AnyAsync(e => e.TraineeProfileId == traineeProfileId && e.CourseId == courseId, ct);
        }

        public async Task<Enrollment> EnrollAsync(Guid traineeProfileId, Guid courseId, decimal pricePaid, CancellationToken ct = default)
        {
            var enrollment = new Enrollment
            {
                TraineeProfileId = traineeProfileId,
                CourseId = courseId,
                PricePaid = pricePaid,
                EnrolledAt = DateTime.UtcNow,
                IsCompleted = false
            };

            await context.Enrollments.AddAsync(enrollment, ct);
            return enrollment;
        }

        public IQueryable<EnrollmentDTO> GetMyEnrollmentsProjected(Guid traineeProfileId)
        {
            // Previously this loaded the full Course -> Modules -> Lessons (including
            // large text columns like Lesson.ArticleContent) plus every LessonProgress
            // row for each enrollment, just to count them in C#. That meant every visit
            // to "My Courses" pulled the entire content of every enrolled course over
            // the wire. Projecting straight to the DTO lets EF Core translate
            // TotalLessons/CompletedLessons into SQL COUNT(...) subqueries, so the
            // database only ever returns the small set of fields we actually display.
            return context.Enrollments
                .AsNoTracking()
                .Where(e => e.TraineeProfileId == traineeProfileId)
                .Select(e => new EnrollmentDTO
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    PricePaid = e.PricePaid,
                    EnrolledAt = e.EnrolledAt,
                    IsCompleted = e.IsCompleted,
                    TotalLessons = e.Course.Modules.SelectMany(m => m.Lessons).Count(),
                    CompletedLessons = e.LessonProgresses.Count(lp => lp.IsCompleted),
                    // ProgressPercent depends on the two counts above and can't be
                    // computed inside the SQL translation; the handler fills it in
                    // in-memory after materializing the (small, paged) result set.
                    ProgressPercent = 0
                });
        }

        public Task<Enrollment?> GetDetailsAsync(Guid enrollmentId, Guid userId, CancellationToken cancellation = default)
        {
            // AsSplitQuery() is important here: this query includes several *sibling* collections
            // (Lessons->LessonResources, EnrollmentModules, Quiz->Attempts, plus LessonProgresses) off
            // the same root. EF Core's default single-query mode joins all of them together, so SQL
            // Server returns a row for every combination (lessons × resources × enrollment-modules ×
            // quiz attempts × lesson-progresses) — a cartesian explosion that grows fast as a course
            // gets more lessons/modules, then EF has to de-duplicate it all in memory. Splitting into
            // separate SELECTs (one per collection) avoids that multiplication entirely.
            return context.Enrollments.AsNoTracking()
                 .Include(e => e.TraineeProfile)
                 .Include(e => e.LessonProgresses)
                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Lessons)
                             .ThenInclude(l => l.LessonResources)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.EnrollmentModules)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Quiz)
                             .ThenInclude(q => q.Attempts)

                 .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.TraineeProfile.UserId == userId, cancellation);
        }

        public Task<Enrollment?> GetByIdWithDetailsAsync(Guid enrollmentId, CancellationToken cancellation = default)
        {
            // NOTE: no .ThenInclude(l => l.LessonResources) here on purpose - this method
            // backs the course player's initial load (EnrollmentDetailsQueryHandler), which
            // only ever reads lesson title/order/progress. Pulling in every LessonResource
            // row (PDFs/videos/blob metadata) for every lesson in the course on every open
            // was pure wasted query + payload weight with nothing downstream using it -
            // that's the actual content is fetched lazily per-lesson via GetLessonByIdQuery.
            return context.Enrollments.AsNoTracking()
                 .Include(e => e.TraineeProfile)
                 .Include(e => e.LessonProgresses)
                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Lessons)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.EnrollmentModules)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Quiz)
                             .ThenInclude(q => q.Attempts)

                 .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellation);
        }

        public Task<Enrollment?> GetDetailsByIdAsync(Guid enrollmentId, CancellationToken cancellation = default)
        {
            return context.Enrollments.AsNoTracking()
                 .Include(e => e.TraineeProfile)
                 .Include(e => e.LessonProgresses)
                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Lessons)
                             .ThenInclude(l => l.LessonResources)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.EnrollmentModules)

                 .Include(e => e.Course)
                     .ThenInclude(c => c.Modules)
                         .ThenInclude(m => m.Quiz)
                             .ThenInclude(q => q.Attempts)

                 .AsSplitQuery()
                 .FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellation);
        }
    }
}
