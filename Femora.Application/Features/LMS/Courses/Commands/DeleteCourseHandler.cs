using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Femora.Domain.Entities;

namespace Femora.Application.Features.LMS.Courses.Commands;

public class DeleteCourseHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly IAppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteCourseHandler(IAppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
        {
            throw new UnauthorizedAccessException("Only Admins can delete courses.");
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new Exception("Course not found");

        var hasEnrollments = await _context.Enrollments
            .AnyAsync(e => e.CourseId == request.CourseId, cancellationToken);

        if (hasEnrollments)
        {
            throw new InvalidOperationException("Cannot delete a course with active enrollments.");
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);
    }
}