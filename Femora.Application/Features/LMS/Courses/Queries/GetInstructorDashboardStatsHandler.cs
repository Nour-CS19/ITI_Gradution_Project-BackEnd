using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Queries
{
    public class GetInstructorDashboardStatsHandler(
        IAppDbContext _context,
        ICurrentUserService _currentUser)
        : IRequestHandler<GetInstructorDashboardStatsQuery, InstructorDashboardStatsDto>
    {
        private const int RecentCoursesCount = 5;

        public async Task<InstructorDashboardStatsDto> Handle(
            GetInstructorDashboardStatsQuery request,
            CancellationToken cancellationToken)
        {
            // EF Core DbContext is NOT thread-safe — every query here runs sequentially,
            // same convention as GetAdminStatsQueryHandler.
            var profile = await _context.InstructorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken);

            if (profile is null)
            {
                // Not yet an instructor (e.g. application still pending) — return an empty,
                // well-formed shape instead of a 404 so the dashboard can render the
                // "apply to become an instructor" quick action.
                return new InstructorDashboardStatsDto
                {
                    VerificationStatus = VerificationStatus.Pending.ToString(),
                    IsVerified = false,
                };
            }

            var coursesQuery = _context.Courses
                .AsNoTracking()
                .Where(c => c.InstructorProfileId == profile.Id);

            var totalCourses = await coursesQuery.CountAsync(cancellationToken);
            var publishedCourses = await coursesQuery.CountAsync(c => c.IsPublished, cancellationToken);
            var pendingCourses = totalCourses - publishedCourses;

            var totalStudents = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Course.InstructorProfileId == profile.Id)
                .Select(e => e.TraineeProfileId)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalEarnings = await _context.InstructorEarnings
                .AsNoTracking()
                .Where(e => e.InstructorProfileId == profile.Id && e.Status == EarningStatus.Paid)
                .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

            var pendingEarnings = await _context.InstructorEarnings
                .AsNoTracking()
                .Where(e => e.InstructorProfileId == profile.Id && e.Status == EarningStatus.Pending)
                .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;

            var recentCourses = await coursesQuery
                .OrderByDescending(c => c.CreatedAt)
                .Take(RecentCoursesCount)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailUrl = c.ThumbnailUrl,
                    Price = c.Price,
                    Category = c.Category,
                    Language = c.Language,
                    Level = c.Level!.ToString(),
                    IsPublished = c.IsPublished,
                    EnrollmentsCount = c.Enrollments.Count(),
                })
                .ToListAsync(cancellationToken);

            return new InstructorDashboardStatsDto
            {
                TotalCourses = totalCourses,
                PublishedCourses = publishedCourses,
                PendingCourses = pendingCourses,
                TotalStudents = totalStudents,
                TotalEarnings = totalEarnings,
                PendingEarnings = pendingEarnings,
                Rating = profile.Rating,
                VerificationStatus = profile.Status.ToString(),
                IsVerified = profile.Status == VerificationStatus.Approved,
                RecentCourses = recentCourses,
            };
        }
    }
}
