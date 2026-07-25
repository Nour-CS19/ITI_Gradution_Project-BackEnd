using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Approvals.Commands.ApproveRequest;

public class ApproveRequestCommandHandler : IRequestHandler<ApproveRequestCommand, bool>
{
    private readonly IAppDbContext _context;

    public ApproveRequestCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        if (approval is null)
            throw new Exception("Approval request not found or already processed.");

        // apply approval based on entity type
        if (approval.Type == ApprovalEntityType.CourseApproval)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == approval.EntityId, cancellationToken);
            if (course is null)
                throw new Exception("Course not found for approval request.");

            course.IsPublished = true;
            course.RequiresApproval = false;
        }

        approval.ApprovalStatus = ApprovalStatus.Approved;
        approval.AdminId = request.AdminId;
        approval.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
