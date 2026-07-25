using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.LMS.Courses.Enums;
using MediatR;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetCoursesQuery : IRequest<PagedResponse<CourseDto>>
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public string? Level { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public CourseSortBy SortBy { get; set; }
        = CourseSortBy.Newest;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}