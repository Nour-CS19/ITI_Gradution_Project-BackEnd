using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.AI;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.AI.Recommendations.Queries.RecommendCourses;

public class RecommendCoursesQueryHandler(IAppDbContext db)
    : IRequestHandler<RecommendCoursesQuery, RecommendCoursesResponse>
{
    public async Task<RecommendCoursesResponse> Handle(RecommendCoursesQuery request, CancellationToken cancellationToken)
    {
        var trainee = await db.TraineeProfiles
            .Include(t => t.PreferredCategories)
                .ThenInclude(pc => pc.CourseCategory)
            .Include(t => t.Enrollments)
            .FirstOrDefaultAsync(t => t.Id == request.TraineeProfileId, cancellationToken)
            ?? throw new NotFoundException("TraineeProfile", request.TraineeProfileId.ToString());

        var preferredCategoryNames = trainee.PreferredCategories
            .Select(pc => pc.CourseCategory.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var enrolledCourseIds = trainee.Enrollments
            .Select(e => e.CourseId)
            .ToHashSet();

        var targetLevel = MapSkillLevelToCourseLevel(trainee.SkillLevel);

        var candidateCourses = await db.Courses
            .Where(c => c.IsPublished && !enrolledCourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        var scored = new List<RecommendedCourseDto>();

        foreach (var course in candidateCourses)
        {
            double score = 0;
            var reasons = new List<string>();

            // Preferred category match - strongest signal
            if (preferredCategoryNames.Contains(course.Category))
            {
                score += 0.5;
                reasons.Add($"Matches your preferred category: {course.Category}");
            }

            // Skill level match
            if (course.Level == targetLevel)
            {
                score += 0.35;
                reasons.Add($"Matches your skill level: {trainee.SkillLevel}");
            }
            else if (course.Level.HasValue && IsAdjacentLevel(targetLevel, course.Level.Value))
            {
                score += 0.15;
                reasons.Add("Close to your current skill level");
            }

            // Slight boost for courses with reviews (social proof), capped contribution
            if (course.Reviews.Count > 0)
            {
                score += Math.Min(0.15, course.Reviews.Count * 0.01);
                reasons.Add("Has trainee reviews");
            }

            if (score <= 0)
                continue;

            scored.Add(new RecommendedCourseDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Category = course.Category,
                Level = course.Level?.ToString() ?? "Unknown",
                Price = course.Price,
                Score = Math.Round(Math.Min(score, 1.0), 2),
                Reasons = reasons
            });
        }

        var topResults = scored
            .OrderByDescending(c => c.Score)
            .Take(request.MaxResults)
            .ToList();

        // Persist as AIRecommendation rows (one per recommended course) for history/tracking.
        foreach (var rec in topResults)
        {
            db.AIRecommendations.Add(new AIRecommendation
            {
                UserId = trainee.UserId,
                Type = AIRecommendationType.Course,
                EntityId = rec.CourseId,
                EntityType = "Course",
                Score = rec.Score,
                ReasonJson = JsonSerializer.Serialize(rec.Reasons),
                GeneratedAt = DateTime.UtcNow
            });
        }

        if (topResults.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return new RecommendCoursesResponse
        {
            TraineeProfileId = trainee.Id,
            Recommendations = topResults
        };
    }

    private static CourseLevel MapSkillLevelToCourseLevel(TrainingSkillLevel skillLevel) => skillLevel switch
    {
        TrainingSkillLevel.Beginner => CourseLevel.Beginner,
        TrainingSkillLevel.Intermediate => CourseLevel.Intermediate,
        TrainingSkillLevel.Proficient => CourseLevel.Intermediate,
        TrainingSkillLevel.Expert => CourseLevel.Advanced,
        TrainingSkillLevel.Master => CourseLevel.Expert,
        _ => CourseLevel.Unknown
    };

    private static bool IsAdjacentLevel(CourseLevel target, CourseLevel actual) =>
        Math.Abs((int)target - (int)actual) == 1;
}
