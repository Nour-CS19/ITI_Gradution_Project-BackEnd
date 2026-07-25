using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Categories.Queries.GetCourseCategories;

/// <summary>
/// Returns every course category (id + name), each annotated with the number of
/// currently-published courses in it - powers the category picker on the
/// "onboarding interests" / "edit my interests" screen.
/// </summary>
public record GetCourseCategoriesQuery : IRequest<List<CourseCategoryDto>>;

public record CourseCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int CourseCount { get; init; }
}
