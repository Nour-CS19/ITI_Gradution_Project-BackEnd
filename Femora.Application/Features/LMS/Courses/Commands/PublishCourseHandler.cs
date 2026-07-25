using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;

namespace Femora.Application.Features.LMS.Courses.Commands;

public class PublishCourseHandler : IRequestHandler<PublishCourseCommand>
{
    private readonly IAppDbContext _context;

    public PublishCourseHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PublishCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new Exception("Course not found");

        if (course.RequiresApproval)
        {
            var latestRequest = await _context.ApprovalRequests
                .Where(x => x.EntityId == course.Id && x.Type == ApprovalEntityType.CourseApproval)
                .OrderByDescending(x => x.RequestedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRequest != null && latestRequest.ApprovalStatus == ApprovalStatus.Rejected)
            {
                latestRequest.ApprovalStatus = ApprovalStatus.Pending;
                latestRequest.RequestedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            throw new InvalidOperationException(
                "This course still requires admin approval before it can be published.");
        }

        course.IsPublished = true;
        course.IsArchived = false; // Publishing un-archives the course

        await _context.SaveChangesAsync(cancellationToken);
    }
}