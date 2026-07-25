using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LessonEntity = Femora.Domain.Entities.LMS.Lesson;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public class CreateLessonHandler : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateLessonHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules
            .FirstOrDefaultAsync(x => x.Id == request.ModuleId, cancellationToken);

        if (module == null)
            throw new Exception("Module not found");

            var courseStatus = await _context.Courses
                .Where(c => c.Id == module.CourseId)
                .Select(c => c.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (courseStatus == CourseStatus.UnderReview)
                throw new InvalidOperationException("Cannot add lessons while the course is under review.");

        var lesson = new LessonEntity
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Title = request.Title,
            ArticleContent = request.ArticleContent,
            ContentUrl = request.ContentUrl,
            DurationSeconds = request.DurationSeconds,
            OrderIndex = request.OrderIndex,
            IsPreview = request.IsPreview
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
}