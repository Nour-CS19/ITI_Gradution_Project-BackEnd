using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Femora.Domain.Entities;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record ArchiveCourseCommand(
    Guid CourseId,
    Guid UserId
) : IRequest;

public class ArchiveCourseHandler : IRequestHandler<ArchiveCourseCommand>
{
    private readonly IAppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ArchiveCourseHandler(IAppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(ArchiveCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.InstructorProfile)
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new Exception("Course not found");

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin)
        {
            if (course.InstructorProfile == null)
                throw new InvalidOperationException("Data integrity error: The course is not linked to any instructor profile.");

            var isOwner = course.InstructorProfile.UserId == request.UserId;
            if (!isOwner)
                throw new UnauthorizedAccessException("You are not authorized to archive this course.");
        }

        course.IsArchived = true;
        course.IsPublished = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
