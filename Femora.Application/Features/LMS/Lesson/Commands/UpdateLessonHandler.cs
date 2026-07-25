using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public class UpdateLessonHandler : IRequestHandler<UpdateLessonCommand>
{
    private readonly IAppDbContext _context;

    public UpdateLessonHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);

        if (lesson == null)
            throw new Exception("Lesson not found");

        var module = await _context.Modules
            .FirstOrDefaultAsync(x => x.Id == lesson.ModuleId, cancellationToken);

        if (module == null)
            throw new Exception("Module not found");

        var courseStatus = await _context.Courses
            .Where(c => c.Id == module.CourseId)
            .Select(c => c.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (courseStatus == CourseStatus.UnderReview)
            throw new InvalidOperationException("Cannot modify lessons while the course is under review.");

        lesson.Title = request.Title;
        lesson.ArticleContent = request.ArticleContent;
        lesson.ContentUrl = request.ContentUrl;
        lesson.DurationSeconds = request.DurationSeconds;
        lesson.OrderIndex = request.OrderIndex;
        lesson.IsPreview = request.IsPreview;

        await _context.SaveChangesAsync(cancellationToken);
    }
}