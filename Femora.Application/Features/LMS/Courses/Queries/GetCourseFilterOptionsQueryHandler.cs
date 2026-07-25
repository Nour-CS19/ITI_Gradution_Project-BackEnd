using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetCourseFilterOptionsQueryHandler(IAppDbContext _context) : IRequestHandler<GetCourseFilterOptionsQuery, CourseFilterOptionsDto>
{
    public async Task<CourseFilterOptionsDto> Handle(GetCourseFilterOptionsQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Courses
            .Where(c => c.IsPublished)
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var levels = await _context.Courses
            .Where(c => c.IsPublished && c.Level.HasValue)
            .Select(c => c.Level!.Value.ToString())
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new CourseFilterOptionsDto
        {
            Categories = categories,
            Levels = levels
        };
    }
}
