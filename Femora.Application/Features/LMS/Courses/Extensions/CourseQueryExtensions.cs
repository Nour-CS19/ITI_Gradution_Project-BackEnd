using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.LMS.Courses.Queries;
using Femora.Domain.Entities.LMS;

namespace Femora.Application.Features.LMS.Courses.Extensions;

public static class CourseQueryExtensions
{
    public static IQueryable<Course> ApplyFilters(
    this IQueryable<Course> query,
    GetCoursesQuery filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            query = query.Where(c =>
                c.Title.Contains(filters.Search) ||
                c.Category.Contains(filters.Search));
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            query = query.Where(c =>
                c.Category == filters.Category);
        }

        if (!string.IsNullOrWhiteSpace(filters.Level))
        {
            query = query.Where(c =>
                c.Level.ToString() == filters.Level);
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(c =>
                c.Price >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(c =>
                c.Price <= filters.MaxPrice.Value);
        }

        return query;
    }
}