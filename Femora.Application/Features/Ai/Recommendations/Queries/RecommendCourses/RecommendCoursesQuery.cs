using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.AI.Recommendations.Queries.RecommendCourses;

/// <summary>
/// Recommends published courses for a trainee based on their SkillLevel,
/// LearningGoals and PreferredCategories. Pure data-driven matching - no AI call.
/// </summary>
public record RecommendCoursesQuery : IRequest<RecommendCoursesResponse>
{
    public Guid TraineeProfileId { get; init; }
    public int MaxResults { get; init; } = 10;
}

public record RecommendCoursesResponse
{
    public Guid TraineeProfileId { get; init; }
    public List<RecommendedCourseDto> Recommendations { get; init; } = new();
}

public record RecommendedCourseDto
{
    public Guid CourseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public double Score { get; init; }
    public List<string> Reasons { get; init; } = new();
}
