using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Queries.GetAdminStats;

public sealed class GetAdminStatsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    public async Task<AdminStatsDto> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        // EF Core DbContext is NOT thread-safe — run every query sequentially.
        // (Task.WhenAll causes "A second operation was started on this context" crash.)

        var totalUsers = await db.ApplicationUsers.CountAsync(cancellationToken);
        var totalTrainees = await db.TraineeProfiles.CountAsync(cancellationToken);
        var totalInstructors = await db.InstructorProfiles.CountAsync(cancellationToken);
        var totalSellers = await db.SellerProfiles.CountAsync(cancellationToken);

        var totalCourses = await db.Courses.CountAsync(cancellationToken);
        var publishedCourses = await db.Courses.CountAsync(c => c.IsPublished, cancellationToken);
        var totalEnroll = await db.Enrollments.CountAsync(cancellationToken);

        var totalProducts = await db.Products.CountAsync(cancellationToken);
        var publishedProd = await db.Products.CountAsync(p => p.IsPuplished, cancellationToken);
        var totalOrders = await db.Orders.CountAsync(cancellationToken);
        var totalRevenue = await db.Payments.SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var pendingApprovals = await db.ApprovalRequests
                                       .CountAsync(a => a.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            TotalTrainees = totalTrainees,
            TotalInstructors = totalInstructors,
            TotalSellers = totalSellers,

            TotalCourses = totalCourses,
            PublishedCourses = publishedCourses,
            TotalEnrollments = totalEnroll,

            TotalProducts = totalProducts,
            PublishedProducts = publishedProd,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,

            PendingApprovals = pendingApprovals,
        };
    }
}