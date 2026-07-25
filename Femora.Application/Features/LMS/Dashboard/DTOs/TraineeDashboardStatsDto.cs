using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Dashboard.DTOs;

/// <summary>
/// Backs the trainee dashboard's stat cards, "متابعة التعلم" progress list,
/// upcoming-quiz card, and achievements — every number here is computed live
/// from Enrollments / QuizAttempts / ProfileApplicationRequests, nothing is hardcoded.
/// </summary>
public class TraineeDashboardStatsDto
{
    // ── Stat cards ───────────────────────────────────────────────────────
    public int PendingRequestsCount { get; init; }
    public int TotalQuizzesCount { get; init; }
    public int CompletedCoursesCount { get; init; }
    public int OngoingCoursesCount { get; init; }

    // ── "متابعة التعلم" — real per-course progress ─────────────────────
    public List<CourseProgressDto> LearningProgress { get; init; } = new();

    // ── "الاختبارات القادمة" — the next quiz actually waiting on the trainee ─
    public UpcomingQuizDto? UpcomingQuiz { get; init; }

    // ── "الإنجازات" — computed from real counts, no hardcoded numbers ──
    public List<AchievementDto> Achievements { get; init; } = new();
}

public class CourseProgressDto
{
    public Guid EnrollmentId { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public int ProgressPercent { get; init; }
}

public class UpcomingQuizDto
{
    public Guid QuizId { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid CourseId { get; init; }
    public string QuizTitle { get; init; } = string.Empty;
    public string CourseTitle { get; init; } = string.Empty;

    // There is no "scheduled date" concept for a quiz in the domain model —
    // a quiz simply becomes takeable the moment its module unlocks. So instead
    // of fabricating a countdown ("in 2 days"), we expose the real unlock date
    // and let the client render "available since X days" / "available now".
    public DateTime? UnlockedAt { get; init; }
    public bool IsAvailableNow { get; init; }

    public int AttemptsUsed { get; init; }
    public int MaxAttempts { get; init; }
}

public class AchievementDto
{
    public string Code { get; init; } = string.Empty;
    public string TitleAr { get; init; } = string.Empty;
    public int CurrentValue { get; init; }
    public int NextThreshold { get; init; }
    public bool IsUnlocked { get; init; }
}
