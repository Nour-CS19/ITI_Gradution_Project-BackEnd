using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.Enrollments.Common.DTOs;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetMyCoursesHandler(
    IAppDbContext _context,
    ICurrentUserService _currentUser)
    : IRequestHandler<GetMyCoursesQuery, PagedResponse<CourseDto>>
{
    public async Task<PagedResponse<CourseDto>> Handle(
        GetMyCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Where(c =>
                c.InstructorProfile.UserId == _currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Title.Contains(request.Search));

        if (request.IsPublished.HasValue)
            query = query.Where(c => c.IsPublished == request.IsPublished.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl,
                Price = c.Price,
                Category = c.Category,
                Language = c.Language,
                IsPublished = c.IsPublished,
                Status = c.Status.ToString(),
                Level = c.Level!.ToString(),
                EnrollmentsCount = c.Enrollments.Count()
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<CourseDto>
        {
            Data = courses,
            Page = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)request.PageSize)
        };
    }
}