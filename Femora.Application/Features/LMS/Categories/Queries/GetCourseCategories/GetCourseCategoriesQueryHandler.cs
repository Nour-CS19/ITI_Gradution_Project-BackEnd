using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Categories.Queries.GetCourseCategories;

public class GetCourseCategoriesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCourseCategoriesQuery, List<CourseCategoryDto>>
{
    public async Task<List<CourseCategoryDto>> Handle(
        GetCourseCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await db.CourseCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CourseCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CourseCount = c.Courses.Count(course => course.IsPublished)
            })
            .ToListAsync(cancellationToken);
    }
}
