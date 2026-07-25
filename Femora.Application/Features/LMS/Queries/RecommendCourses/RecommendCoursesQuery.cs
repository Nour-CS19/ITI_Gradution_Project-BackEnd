using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Queries.RecommendCourses;

public record RecommendCoursesQuery : IRequest<List<RecommendedCourseDto>>
{
    public Guid UserId { get; init; }
    public int Top { get; init; } = 10;
}

public record RecommendedCourseDto
{
    public Guid CourseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string Level { get; init; } = string.Empty;
    public double Score { get; init; }
}
