using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.LMS.Lesson.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Lesson.Queries.GetLessonById;

public class GetLessonByIdHandler(IAppDbContext context, IBlobStorageRepository blobStorage, ICurrentUserService currentUserService)
    : IRequestHandler<GetLessonByIdQuery, LessonDetailsDto>
{
    public async Task<LessonDetailsDto> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lessonEntity = await context.Lessons
            .Include(l => l.Module)
                .ThenInclude(m => m.Course)
                    .ThenInclude(c => c.InstructorProfile)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken);

        if (lessonEntity is null)
            throw new NotFoundException("Lesson", request.LessonId.ToString());

        if (!lessonEntity.IsPreview)
        {
            var currentUserId = currentUserService.UserId;
            if (currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("You must be logged in to view this lesson.");
            }

            // Fast path: the instructor who owns the course needs no DB round-trip at all -
            // Module/Course/InstructorProfile are already in memory from the Include above.
            var isOwner = lessonEntity.Module?.Course?.InstructorProfile?.UserId == currentUserId;

            if (!isOwner)
            {
                var courseId = lessonEntity.Module.CourseId;

                // Admin + enrollment checks used to be three separate sequential
                // round-trips (admin role lookup, trainee-profile lookup, enrollment
                // lookup). EF Core translates the two Any(...) subqueries below into
                // a single SQL statement (EXISTS clauses), so this is now one round-trip.
                var access = await context.ApplicationUsers
                    .AsNoTracking()
                    .Where(u => u.Id == currentUserId)
                    .Select(u => new
                    {
                        IsAdmin = context.UserRoles.Any(ur =>
                            ur.UserId == currentUserId &&
                            context.ApplicationRoles.Any(r => r.Id == ur.RoleId && r.Name == "Admin")),
                        IsEnrolled = context.Enrollments.Any(e =>
                            e.TraineeProfile.UserId == currentUserId && e.CourseId == courseId),
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                var isAdmin = access?.IsAdmin ?? false;
                var isEnrolled = access?.IsEnrolled ?? false;

                if (!isAdmin && !isEnrolled)
                {
                    throw new UnauthorizedAccessException("You are not enrolled in this course.");
                }
            }
        }

        var lessonDto = new LessonDetailsDto
        {
            Id = lessonEntity.Id,
            ModuleId = lessonEntity.ModuleId,
            Title = lessonEntity.Title,
            Type = lessonEntity.Type,
            ArticleContent = lessonEntity.ArticleContent,
            ContentUrl = lessonEntity.ContentUrl,
            DurationSeconds = lessonEntity.DurationSeconds ?? 0,
            OrderIndex = lessonEntity.OrderIndex,
            IsPreview = lessonEntity.IsPreview
        };

        lessonDto.ContentUrl = ToAccessibleLessonUrl(lessonDto.ContentUrl, blobStorage);

        return lessonDto;
    }

    private static string? ToAccessibleLessonUrl(string? contentUrl, IBlobStorageRepository blobStorage)
    {
        if (string.IsNullOrWhiteSpace(contentUrl))
            return contentUrl;

        var normalized = contentUrl.Trim();

        try
        {
            if (normalized.StartsWith("lesson-resources/", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("/lesson-resources/", StringComparison.OrdinalIgnoreCase))
            {
                return blobStorage.GetSasUrl(normalized.TrimStart('/'), TimeSpan.FromHours(2));
            }

            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
                return contentUrl;

            var isBlobUrl = uri.Host.Contains("blob", StringComparison.OrdinalIgnoreCase)
                || uri.AbsolutePath.Contains("lesson-resources", StringComparison.OrdinalIgnoreCase);

            if (!isBlobUrl)
                return contentUrl;

            var blobName = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            if (string.IsNullOrWhiteSpace(blobName))
                return contentUrl;

            return blobStorage.GetSasUrl(blobName, TimeSpan.FromHours(2));
        }
        catch
        {
            return contentUrl;
        }
    }
}
