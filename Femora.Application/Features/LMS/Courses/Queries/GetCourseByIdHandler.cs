using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.LMS.Lesson.DTOs;
using Femora.Application.Features.LMS.Modules.DTOs;
using Femora.Domain.Entities.LMS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetCourseByIdHandler(IAppDbContext _context) : IRequestHandler<GetCourseByIdQuery, CourseDetailsDto>
{
    public async Task<CourseDetailsDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
         .AsNoTracking()
         .Where(c => c.Id == request.Id &&
                  (c.IsPublished
                   || request.RequestingUserIsAdmin
                   || c.InstructorProfile.UserId == request.RequestingUserId))
         .Select(c => new CourseDetailsDto
         {
             Id = c.Id,
             InstructorProfileId = c.InstructorProfileId,
             Title = c.Title,
             Description = c.Description,
             ThumbnailUrl = c.ThumbnailUrl,
             Price = c.Price,
             Category = c.Category,
             Language = c.Language,
             Level = c.Level.HasValue ? c.Level.Value.ToString() : string.Empty,
             IsPublished = c.IsPublished,
             Status = c.Status.ToString(),
             CreatedAt = c.CreatedAt,
             UpdatedAt = c.UpdatedAt,

             InstructorName = c.InstructorProfile.User.FirstName + " " + c.InstructorProfile.User.LastName,
             EnrollmentsCount = c.Enrollments.Count(),
             TotalLessons = c.Modules.SelectMany(m => m.Lessons).Count(),

             Modules = c.Modules
                .OrderBy(m => m.OrderIndex)
                .Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    OrderIndex = m.OrderIndex,
                     LessonsCount = m.Lessons.Count,
                    CourseId = m.CourseId,
                    Lessons = m.Lessons
                        .OrderBy(l => l.OrderIndex)
                        .Select(l => new LessonDto
                        {
                            Id = l.Id,
                            ModuleId = l.ModuleId,
                            Title = l.Title,
                            Type = l.Type,
                            ArticleContent = l.ArticleContent,
                            ContentUrl = l.ContentUrl,
                            DurationSeconds = l.DurationSeconds ?? 0,
                            OrderIndex = l.OrderIndex,
                            IsPreview = l.IsPreview
                        }).ToList(),
                })
                .ToList()
         })
         .FirstOrDefaultAsync(cancellationToken)
         ?? throw new NotFoundException(
             nameof(Course),
             request.Id.ToString());

        return course;
    }
}