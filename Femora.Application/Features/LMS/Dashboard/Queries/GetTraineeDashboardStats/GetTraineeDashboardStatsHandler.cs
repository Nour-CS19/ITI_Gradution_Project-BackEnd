using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Dashboard.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Dashboard.Queries.GetTraineeDashboardStats;

public class GetTraineeDashboardStatsHandler(
    IAppDbContext _context,
    ICurrentUserService _currentUser)
    : IRequestHandler<GetTraineeDashboardStatsQuery, TraineeDashboardStatsDto>
{
    private const int LearningProgressCount = 3;

    public async Task<TraineeDashboardStatsDto> Handle(
        GetTraineeDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var traineeProfile = await _context.TraineeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == _currentUser.UserId, cancellationToken);

        if (traineeProfile is null)
        {
            // Not a trainee yet — return an empty, well-formed shape instead of a 404
            // so the dashboard can still render (same convention as the instructor
            // dashboard stats handler).
            return new TraineeDashboardStatsDto();
        }

        // ── Pending requests card ("الطلبات") ───────────────────────────
        var pendingRequestsCount = await _context.ProfileApplicationRequests
            .AsNoTracking()
            .CountAsync(r => r.UserId == _currentUser.UserId && r.Status == ApplicationRequestStatus.Pending,
                cancellationToken);

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.TraineeProfileId == traineeProfile.Id)
            .Select(e => new
            {
                e.Id,
                e.CourseId,
                CourseTitle = e.Course.Title,
                e.IsCompleted,
                e.EnrolledAt,
                TotalLessons = e.Course.Modules.SelectMany(m => m.Lessons).Count(),
                CompletedLessons = e.LessonProgresses.Count(lp => lp.IsCompleted),
            })
            .ToListAsync(cancellationToken);

        var completedCoursesCount = enrollments.Count(e => e.IsCompleted);
        var ongoingCoursesCount = enrollments.Count(e => !e.IsCompleted);
        var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();
        var enrollmentIds = enrollments.Select(e => e.Id).ToHashSet();

        var totalQuizzesCount = await _context.Quizzes
            .AsNoTracking()
            .CountAsync(q => enrolledCourseIds.Contains(q.CourseId), cancellationToken);

        var learningProgress = enrollments
            .Where(e => !e.IsCompleted)
            .OrderByDescending(e => e.EnrolledAt)
            .Take(LearningProgressCount)
            .Select(e => new CourseProgressDto
            {
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.CourseTitle,
                ProgressPercent = CalculateProgress(e.CompletedLessons, e.TotalLessons),
            })
            .ToList();

        var candidateModules = await _context.EnrollmentModules
            .AsNoTracking()
            .Where(em => enrollmentIds.Contains(em.EnrollmentId) && em.IsUnlocked && em.Module.QuizId.HasValue && em.Module.Quiz != null)
            .Select(em => new
            {
                em.EnrollmentId,
                em.UnlockedAt,
                em.Module.OrderIndex,
                QuizId = em.Module.QuizId!.Value,
                QuizTitle = em.Module.Quiz.Title,
                MaxAttempts = em.Module.Quiz.MaxAttempts,
                CourseId = em.Module.CourseId,
            })
            .ToListAsync(cancellationToken);

        UpcomingQuizDto? upcomingQuiz = null;

        if (candidateModules.Count > 0)
        {
            var candidateQuizIds = candidateModules.Select(c => c.QuizId).ToHashSet();

            var attempts = await _context.QuizAttempts
                .AsNoTracking()
                .Where(a => candidateQuizIds.Contains(a.QuizId) && enrollmentIds.Contains(a.EnrollmentId))
                .Select(a => new { a.QuizId, a.EnrollmentId, a.IsPassed })
                .ToListAsync(cancellationToken);

            var attemptsByPair = attempts
                .GroupBy(a => (a.QuizId, a.EnrollmentId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var next = candidateModules
                .Where(c => !(attemptsByPair.TryGetValue((c.QuizId, c.EnrollmentId), out var list)
                              && list.Any(a => a.IsPassed)))
                .OrderBy(c => c.OrderIndex)
                .ThenBy(c => c.UnlockedAt)
                .FirstOrDefault();

            if (next is not null)
            {
                var attemptsUsed = attemptsByPair.TryGetValue((next.QuizId, next.EnrollmentId), out var used)
                    ? used.Count
                    : 0;

                var courseTitle = enrollments.FirstOrDefault(e => e.Id == next.EnrollmentId)?.CourseTitle ?? string.Empty;

                upcomingQuiz = new UpcomingQuizDto
                {
                    QuizId = next.QuizId,
                    EnrollmentId = next.EnrollmentId,
                    CourseId = next.CourseId,
                    QuizTitle = next.QuizTitle,
                    CourseTitle = courseTitle,
                    UnlockedAt = next.UnlockedAt,
                    IsAvailableNow = true,
                    AttemptsUsed = attemptsUsed,
                    MaxAttempts = next.MaxAttempts,
                };
            }
        }

        var passedQuizzesCount = await _context.QuizAttempts
            .AsNoTracking()
            .Where(a => a.TraineeProfileId == traineeProfile.Id && a.IsPassed)
            .Select(a => a.QuizId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalCompletedLessons = enrollments.Sum(e => e.CompletedLessons);

        var achievements = new List<AchievementDto>
        {
            BuildAchievement("active_learner", "متعلم نشط", completedCoursesCount, new[] { 1, 3, 5, 10 }),
            BuildAchievement("quiz_master", "بطل الاختبارات", passedQuizzesCount, new[] { 1, 5, 10, 20 }),
            BuildAchievement("consistent_learner", "متعلم مثابر", totalCompletedLessons, new[] { 10, 25, 50, 100 }),
        };

        return new TraineeDashboardStatsDto
        {
            PendingRequestsCount = pendingRequestsCount,
            TotalQuizzesCount = totalQuizzesCount,
            CompletedCoursesCount = completedCoursesCount,
            OngoingCoursesCount = ongoingCoursesCount,
            LearningProgress = learningProgress,
            UpcomingQuiz = upcomingQuiz,
            Achievements = achievements,
        };
    }

    private static int CalculateProgress(int completedLessons, int totalLessons)
    {
        if (totalLessons == 0)
            return 0;

        return (int)Math.Round((double)completedLessons / totalLessons * 100);
    }

    private static AchievementDto BuildAchievement(string code, string titleAr, int value, int[] thresholds)
    {
        var isUnlocked = value >= thresholds[0];
        var nextThreshold = thresholds.FirstOrDefault(t => t > value);
        if (nextThreshold == 0)
            nextThreshold = thresholds[^1]; 

        return new AchievementDto
        {
            Code = code,
            TitleAr = titleAr,
            CurrentValue = value,
            NextThreshold = nextThreshold,
            IsUnlocked = isUnlocked,
        };
    }
}
