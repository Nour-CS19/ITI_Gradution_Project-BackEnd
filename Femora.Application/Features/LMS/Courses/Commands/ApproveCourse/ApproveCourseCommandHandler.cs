using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Commands.ApproveCourse;

public class ApproveCourseCommandHandler(IAppDbContext db)
    : IRequestHandler<ApproveCourseCommand>
{
    public async Task Handle(
        ApproveCourseCommand request,
        CancellationToken cancellationToken)
    {
        var course = await db.Courses
            .FirstOrDefaultAsync(
                x => x.Id == request.CourseId,
                cancellationToken);

        if (course is null)
        {
            throw new NotFoundException(
                nameof(Course),
                request.CourseId.ToString());
        }

        course.IsPublished = true;
        course.RequiresApproval = false;
        course.Status = CourseStatus.Published;

        var approvalRequest = await db.ApprovalRequests
            .Where(x => x.EntityId == request.CourseId
                && x.Type == ApprovalEntityType.CourseApproval
                && x.ApprovalStatus == ApprovalStatus.Pending)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (approvalRequest is not null)
        {
            approvalRequest.ApprovalStatus = ApprovalStatus.Approved;
            approvalRequest.AdminId = request.AdminId;
            approvalRequest.ReviewedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
