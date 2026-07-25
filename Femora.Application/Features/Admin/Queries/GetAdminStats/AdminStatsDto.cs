namespace Femora.Application.Features.Admin.Queries.GetAdminStats;

public class AdminStatsDto
{
    // ── Users ──────────────────────────────────────────────────────────────
    public int TotalUsers { get; init; }
    public int TotalTrainees { get; init; }
    public int TotalInstructors { get; init; }
    public int TotalSellers { get; init; }

    // ── LMS ────────────────────────────────────────────────────────────────
    public int TotalCourses { get; init; }
    public int PublishedCourses { get; init; }
    public int TotalEnrollments { get; init; }

    // ── Marketplace ────────────────────────────────────────────────────────
    public int TotalProducts { get; init; }
    public int PublishedProducts { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalRevenue { get; init; }

    // ── Pending ────────────────────────────────────────────────────────────
    public int PendingApprovals { get; init; }
}