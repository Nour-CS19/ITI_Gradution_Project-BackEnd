using Femora.Application.Features.LMS.Courses.Enums;
using Femora.Domain.Entities.LMS;

namespace Femora.Application.Features.LMS.Courses.Extensions;

public static class CourseSortingExtensions
{
    public static IQueryable<Course> ApplySorting(
        this IQueryable<Course> query,
        CourseSortBy sortBy)
    {
        return sortBy switch
        {
            CourseSortBy.Oldest =>
                query.OrderBy(c => c.CreatedAt),

            CourseSortBy.PriceLowToHigh =>
                query.OrderBy(c => c.Price),

            CourseSortBy.PriceHighToLow =>
                query.OrderByDescending(c => c.Price),

            CourseSortBy.MostPopular =>
                query.OrderByDescending(c =>
                    c.Enrollments.Count),

            _ =>
                query.OrderByDescending(c => c.CreatedAt)
        };
    }
}