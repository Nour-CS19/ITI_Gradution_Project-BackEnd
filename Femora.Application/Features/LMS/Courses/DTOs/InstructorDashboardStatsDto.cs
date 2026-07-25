using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Courses.DTOs
{
    /// <summary>
    /// Backs the instructor dashboard's statistics cards, recent-courses section,
    /// and quick-actions gating (e.g. hide "create course" if not yet verified).
    /// </summary>
    public class InstructorDashboardStatsDto
    {
        // ── Statistics cards ─────────────────────────────────────────────────
        public int TotalCourses { get; init; }
        public int PublishedCourses { get; init; }
        public int PendingCourses { get; init; }
        public int TotalStudents { get; init; }
        public decimal TotalEarnings { get; init; }
        public decimal PendingEarnings { get; init; }
        public float Rating { get; init; }

        // ── Verification / quick actions ────────────────────────────────────
        public string VerificationStatus { get; init; } = string.Empty;
        public bool IsVerified { get; init; }

        // ── Recent courses section ──────────────────────────────────────────
        public List<CourseDto> RecentCourses { get; init; } = new();
    }
}
