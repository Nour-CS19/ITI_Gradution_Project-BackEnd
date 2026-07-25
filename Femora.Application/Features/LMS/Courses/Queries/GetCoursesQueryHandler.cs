using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.LMS.Courses.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetCoursesQueryHandler(IAppDbContext _context) : IRequestHandler<GetCoursesQuery, PagedResponse<CourseDto>>
{
    public async Task<PagedResponse<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Courses.AsNoTracking()
                    .Where(c => c.IsPublished).ApplyFilters(request);

        var totalCount = await query.CountAsync(cancellationToken);

        var courses = await query
            .ApplySorting(request.SortBy)
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
                Level = c.Level.HasValue
                    ? c.Level.Value.ToString()
                    : string.Empty,
                InstructorName =
                    c.InstructorProfile.User.FirstName + " " +
                    c.InstructorProfile.User.LastName,
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