using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Exceptions;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Femora.Application.Features.Approvals.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record RejectCourseCommand(
    Guid CourseId,
    Guid AdminId,
    string Reason
) : IRequest;

public class RejectCourseHandler : IRequestHandler<RejectCourseCommand>
{
    private readonly IAppDbContext _context;

    public RejectCourseHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new NotFoundException(nameof(Course), request.CourseId.ToString());

        course.Status = CourseStatus.Rejected;
        course.RequiresApproval = false;
        course.IsPublished = false;

        var approvalRequest = await _context.ApprovalRequests
            .Where(x => x.EntityId == request.CourseId
                && x.Type == ApprovalEntityType.CourseApproval
                && x.ApprovalStatus == ApprovalStatus.Pending)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (approvalRequest != null)
        {
            approvalRequest.ApprovalStatus = ApprovalStatus.Rejected;
            approvalRequest.AdminId = request.AdminId;
            approvalRequest.ReviewedAt = DateTime.UtcNow;

            var payload = ApprovalNotePayload.Parse(approvalRequest.Note);
            payload.AdminNote = request.Reason;
            approvalRequest.Note = payload.ToJson();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
