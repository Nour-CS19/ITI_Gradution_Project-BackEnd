using Femora.Application.Features.LMS.Courses.Enums;

namespace Femora.Application.Features.LMS.Courses.DTOs;

public class CourseFilterDto
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public string? Level { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }
    public CourseSortBy SortBy { get; set; }
    = CourseSortBy.Newest;
}
