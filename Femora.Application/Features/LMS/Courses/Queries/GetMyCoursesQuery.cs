using MediatR;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.Enrollments.Common.DTOs;

namespace Femora.Application.Features.LMS.Courses.Queries;

public class GetMyCoursesQuery : IRequest<PagedResponse<CourseDto>>
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public bool? IsPublished { get; set; }
}