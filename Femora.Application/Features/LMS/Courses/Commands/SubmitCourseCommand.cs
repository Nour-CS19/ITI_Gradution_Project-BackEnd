using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Exceptions;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Femora.Application.Features.Approvals.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record SubmitCourseCommand(
    Guid CourseId,
    Guid UserId
) : IRequest;

public class SubmitCourseHandler : IRequestHandler<SubmitCourseCommand>
{
    private readonly IAppDbContext _context;

    public SubmitCourseHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SubmitCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.InstructorProfile)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new NotFoundException(nameof(Course), request.CourseId.ToString());

        if (course.InstructorProfile == null || course.InstructorProfile.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to submit this course.");

        if (course.Status == CourseStatus.UnderReview)
            throw new InvalidOperationException("Course is already under review.");

        if (course.Modules.Count == 0)
            throw new InvalidOperationException("يجب إضافة وحدة واحدة على الأقل قبل الإرسال");

        foreach (var module in course.Modules)
        {
            if (module.Lessons.Count == 0)
            {
                throw new InvalidOperationException($"الوحدة '{module.Title}' لا تحتوي على أي درس");
            }
        }

        course.Status = CourseStatus.UnderReview;
        course.RequiresApproval = true;
        course.IsPublished = false;

        // Insert new Pending ApprovalRequest
        var approvalRequest = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            RequsterId = course.InstructorProfile.UserId,
            EntityId = course.Id,
            Type = ApprovalEntityType.CourseApproval,
            ApprovalStatus = ApprovalStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            Note = new ApprovalNotePayload
            {
                Title = course.Title,
                Description = course.Description
            }.ToJson()
        };

        await _context.ApprovalRequests.AddAsync(approvalRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
