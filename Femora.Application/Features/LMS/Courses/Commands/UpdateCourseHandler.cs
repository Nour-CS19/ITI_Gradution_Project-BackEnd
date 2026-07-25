using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;

namespace Femora.Application.Features.LMS.Courses.Commands;

public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly IAppDbContext _context;

    public UpdateCourseHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new Exception("Course not found");

        if (course.Status == CourseStatus.UnderReview)
            throw new InvalidOperationException("Cannot modify course details while the course is under review.");

        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = request.Price;
        course.Category = request.Category;
        course.Level = request.Level;
        course.Language = request.Language;
        course.ThumbnailUrl = request.ThumbnailUrl;

        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}