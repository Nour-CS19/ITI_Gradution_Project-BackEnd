using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Entities.AI;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Queries.RecommendCourses;

public class RecommendCoursesQueryHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository)
    : IRequestHandler<RecommendCoursesQuery, List<RecommendedCourseDto>>
{
    private const int CandidatePoolSize = 50;

    // Final score = EmbeddingWeight * semantic similarity + CategoryMatchWeight * keyword
    // overlap between the trainee's interests and the course's category/title. Pure
    // embedding similarity alone can let a lexically-close-but-wrong-category course
    // (e.g. "Jewelry") outrank an actual match (e.g. "Crochet") when descriptions share
    // a lot of generic handicraft vocabulary. The keyword term acts as a strong, cheap
    // "does this course literally belong to what they said they're interested in" signal
    // that anchors the ranking, while the embedding still supplies the nuance (surfacing
    // relevant courses whose category *isn't* an exact keyword match).
    private const double EmbeddingWeight = 0.65;
    private const double CategoryMatchWeight = 0.35;

    // How confident the keyword overlap must be before a course counts as "belonging to"
    // a specific interest for diversification purposes. Below this, treat it as unmatched
    // rather than forcing it into a weakly-related interest bucket.
    private const double MinBucketMatchScore = 0.05;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "و", "في", "من", "على", "مع", "عن", "the", "and", "of", "for", "&", "a", "an",
    };

    public async Task<List<RecommendedCourseDto>> Handle(RecommendCoursesQuery request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .Include(u => u.OnboardingInterests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var preferredInterests = user is null
            ? new List<string>()
            : user.OnboardingInterests
                .Select(i => $"{i.NameEn} ({i.NameAr})")
                .ToList();

        // One token set per selected interest (name + description, AR + EN), used purely
        // for the keyword-overlap boost below — kept separate from the embedding prompt.
        var interestTokenSets = user is null
            ? new List<HashSet<string>>()
            : user.OnboardingInterests
                .Select(i => Tokenize($"{i.NameAr} {i.NameEn} {i.DescriptionAr} {i.DescriptionEn}"))
                .Where(set => set.Count > 0)
                .ToList();

        // Try to find TraineeProfile — Buyer users won't have one, that's fine.
        var traineeProfile = await db.TraineeProfiles
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        // Courses the user is already enrolled in (only applies if they have a trainee profile)
        List<Guid> enrolledCourseIds = new();
        if (traineeProfile is not null)
        {
            enrolledCourseIds = await db.Enrollments
                .Where(e => e.TraineeProfileId == traineeProfile.Id)
                .Select(e => e.CourseId)
                .ToListAsync(cancellationToken);
        }

        var eligibleCourses = await db.Courses
            .Where(c => c.IsPublished && !enrolledCourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (eligibleCourses.Count == 0)
            return new List<RecommendedCourseDto>();

        // Keyword match, computed once per eligible course up front so it can (a) decide
        // which courses make the candidate pool and (b) contribute to the final blended
        // score below, without tokenizing the same course twice.
        var categoryMatchByCourseId = eligibleCourses.ToDictionary(
            c => c.Id,
            c => CategoryMatchScore(c, interestTokenSets));

        // Category-matching courses always get a seat in the candidate pool regardless of
        // how recently they were published; remaining slots are topped up by recency, same
        // as before. This matters once the catalog grows past CandidatePoolSize — otherwise
        // a genuinely relevant course could be excluded from consideration entirely just
        // because it wasn't one of the most recently created.
        var candidates = eligibleCourses
            .OrderByDescending(c => categoryMatchByCourseId[c.Id])
            .ThenByDescending(c => c.CreatedAt)
            .Take(CandidatePoolSize)
            .ToList();

        // Build interest text — for Buyer/new user use a generic popular-courses prompt
        var interestText = preferredInterests.Count > 0
            ? $"Courses related to: {string.Join(", ", preferredInterests)}"
            : "Popular handcraft and home business courses for women in Egypt";

        var interestEmbedding = await embeddingRepository.GenerateEmbeddingAsync(interestText, cancellationToken);

        var courseTexts = candidates
            .Select(c => $"{c.Title}. {c.Description ?? string.Empty}")
            .ToList();

        var courseEmbeddings = await embeddingRepository.GenerateEmbeddingsAsync(courseTexts, cancellationToken);

        var scoredCandidates = candidates
            .Zip(courseEmbeddings, (course, embedding) =>
            {
                var semanticScore = CosineSimilarity(interestEmbedding, embedding);
                var categoryScore = categoryMatchByCourseId[course.Id];
                var blendedScore = (EmbeddingWeight * semanticScore) + (CategoryMatchWeight * categoryScore);
                var bucket = BestInterestBucket(course, interestTokenSets);

                return (Course: course, Score: blendedScore, Bucket: bucket);
            })
            .ToList();

        // With more than one selected interest, spread the results across interests
        // instead of letting whichever interest scores highest dominate every slot —
        // picking "Crochet" and "Embroidery" should surface courses from BOTH (when
        // both have eligible courses), not 3 crochet courses and zero embroidery ones
        // just because crochet happened to score a bit higher overall.
        var ranked = interestTokenSets.Count > 1
            ? SelectDiversified(scoredCandidates, request.Top)
            : scoredCandidates.OrderByDescending(x => x.Score).Take(request.Top).ToList();

        // Persist recommendations for tracking/analytics
        foreach (var item in ranked)
        {
            db.AIRecommendations.Add(new AIRecommendation
            {
                UserId = request.UserId,
                Type = AIRecommendationType.Course,
                EntityId = item.Course.Id,
                EntityType = "Course",
                IsViewed = false,
                GeneratedAt = DateTime.UtcNow,
                Score = item.Score
            });
        }
        await db.SaveChangesAsync(cancellationToken);

        return ranked.Select(item => new RecommendedCourseDto
        {
            CourseId = item.Course.Id,
            Title = item.Course.Title,
            Description = item.Course.Description,
            CategoryName = item.Course.Category,
            Price = item.Course.Price,
            ThumbnailUrl = item.Course.ThumbnailUrl,
            Level = item.Course.Level?.ToString() ?? string.Empty,
            Score = item.Score
        }).ToList();
    }

    private static double CategoryMatchScore(Course course, List<HashSet<string>> interestTokenSets)
    {
        if (interestTokenSets.Count == 0)
            return 0;

        var courseTokens = Tokenize($"{course.Category} {course.Title}");
        if (courseTokens.Count == 0)
            return 0;

        // A course only needs to match ONE of the trainee's selected interests to get
        // full credit — take the best match across interests rather than averaging them
        // (averaging would unfairly penalize a course for not matching interests the
        // trainee also picked but that this course has nothing to do with).
        return interestTokenSets.Max(interestTokens => JaccardOverlap(interestTokens, courseTokens));
    }

    /// <summary>
    /// Which selected interest (by index into interestTokenSets) this course best matches,
    /// or null if it doesn't clearly belong to any one of them. Used only to group
    /// candidates for diversification — the actual ranking score still comes from
    /// CategoryMatchScore + the embedding.
    /// </summary>
    private static int? BestInterestBucket(Course course, List<HashSet<string>> interestTokenSets)
    {
        if (interestTokenSets.Count == 0)
            return null;

        var courseTokens = Tokenize($"{course.Category} {course.Title}");
        if (courseTokens.Count == 0)
            return null;

        var bestIndex = -1;
        var bestScore = 0.0;

        for (var i = 0; i < interestTokenSets.Count; i++)
        {
            var score = JaccardOverlap(interestTokenSets[i], courseTokens);
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestScore >= MinBucketMatchScore ? bestIndex : (int?)null;
    }

    /// <summary>
    /// Round-robins one course per interest bucket (each internally sorted by score) until
    /// every bucket is exhausted or `top` slots are filled, then tops up any remaining
    /// slots from courses that didn't clearly match a single interest. This guarantees
    /// every selected interest gets a seat (when it has eligible courses at all) instead
    /// of the highest-scoring interest crowding out the others.
    /// </summary>
    private static List<(Course Course, double Score, int? Bucket)> SelectDiversified(
        List<(Course Course, double Score, int? Bucket)> scored,
        int top)
    {
        if (top <= 0 || scored.Count == 0)
            return new List<(Course Course, double Score, int? Bucket)>();

        // Dictionary<TKey,TValue> throws ArgumentNullException on a null key even when
        // TKey is a nullable value type like int? — so the "unmatched" group (Bucket ==
        // null) can never go through ToDictionary(g => g.Key, ...) directly. Split it out
        // into its own queue up front instead of trying to key a dictionary with null.
        var interestBuckets = scored
            .Where(x => x.Bucket.HasValue)
            .GroupBy(x => x.Bucket!.Value)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => new Queue<(Course Course, double Score, int? Bucket)>(g.OrderByDescending(x => x.Score)));

        var unmatchedQueue = new Queue<(Course Course, double Score, int? Bucket)>(
            scored.Where(x => !x.Bucket.HasValue).OrderByDescending(x => x.Score));

        var bucketOrder = interestBuckets.Keys.ToList();

        var result = new List<(Course Course, double Score, int? Bucket)>();

        var progress = true;
        while (result.Count < top && progress)
        {
            progress = false;
            foreach (var key in bucketOrder)
            {
                if (result.Count >= top) break;
                if (interestBuckets[key].Count > 0)
                {
                    result.Add(interestBuckets[key].Dequeue());
                    progress = true;
                }
            }
        }

        // Fill any leftover slots from courses that didn't clearly belong to one
        // interest but still scored reasonably overall (mostly via the embedding).
        while (result.Count < top && unmatchedQueue.Count > 0)
            result.Add(unmatchedQueue.Dequeue());

        return result;
    }

    private static HashSet<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var separators = new[] { ' ', ',', '.', '،', '(', ')', '-', '/', '\n', '\r', '\t' };

        var tokens = text
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeToken)
            .Where(t => t.Length > 1 && !StopWords.Contains(t));

        return new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeToken(string token)
    {
        token = token.Trim().ToLowerInvariant();

        // Strip the Arabic definite article ("ال") so "الكروشيه" and "كروشيه" are treated
        // as the same term — course categories and interest names don't always agree on
        // whether to include it.
        if (token.Length > 3 && token.StartsWith("ال", StringComparison.Ordinal))
            token = token[2..];

        return token;
    }

    private static double JaccardOverlap(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0)
            return 0;

        var intersection = a.Count(b.Contains);
        if (intersection == 0)
            return 0;

        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0 : (double)intersection / union;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
