using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.LMS;
using MediatR;

namespace Femora.Application.Features.Enrollments.Queries.EnrollmentDetails;

public class EnrollmentDetailsQueryHandler(
    ICurrentUserService currentUser,
    IEnrollmentRepository enrollmentRepo)
    : IRequestHandler<EnrollmentDetailsQuery, EnrollmentDetailsResponse>
{
    public async Task<EnrollmentDetailsResponse> Handle(EnrollmentDetailsQuery request, CancellationToken cancellationToken)
    {
        // Retrieve the enrollment without applying user-based filtering so we can
        // distinguish between a missing enrollment and a forbidden access.
        var enrollment = await enrollmentRepo.GetByIdWithDetailsAsync(request.EnrollmentId, cancellationToken);

        if (enrollment == null)
        {
            throw new NotFoundException("Enrollment", request.EnrollmentId.ToString());
        }

        if (enrollment.TraineeProfile == null || enrollment.TraineeProfile.UserId != currentUser.UserId)
            throw new ForbiddenException("You don't have access to this enrollment.");

        return BuildResponse(enrollment);
    }

    private static EnrollmentDetailsResponse BuildResponse(Enrollment enrollment)
    {
        var totalLessons = enrollment.Course.Modules.SelectMany(m => m.Lessons).Count();
        var completedLessons = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);
        var progressLookup = enrollment.LessonProgresses.ToDictionary(lp => lp.LessonId);

        return new EnrollmentDetailsResponse
        {
            EnrollmentId = enrollment.Id,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course.Title,
            ThumbnailUrl = enrollment.Course.ThumbnailUrl,
            IsCompleted = enrollment.IsCompleted,
            ProgressPercent = CalculateProgressPercent(completedLessons, totalLessons),
            Modules = enrollment.Course.Modules.OrderBy(m => m.OrderIndex)
                .Select((m, index) => MapModule(m, enrollment, progressLookup, isFirstModule: index == 0)).ToList(),
        };
    }

    private static int CalculateProgressPercent(int completedLessons, int totalLessons)
    {
        if (totalLessons == 0) return 0;

        return (int)Math.Round((double)completedLessons / totalLessons * 100);
    }

    private static ModuleDetailsDto MapModule(Module module, Enrollment enrollment, Dictionary<Guid, LessonProgress> progressLookup, bool isFirstModule)
    {
        var enrollmentModule = module.EnrollmentModules.FirstOrDefault(em => em.EnrollmentId == enrollment.Id);
        var quizPassed = module.Quiz?.Attempts.Any(a => a.EnrollmentId == enrollment.Id && a.IsPassed) ?? false;

        var lessons = module.Lessons.OrderBy(l => l.OrderIndex)
            .Select(l => MapLesson(l, progressLookup)).ToList();

        var allLessonsCompleted = lessons.All(l => l.IsCompleted);
        var hasQuiz = module.Quiz is not null;

        return new ModuleDetailsDto
        {
            ModuleId = module.Id,
            Title = module.Title,
            OrderIndex = module.OrderIndex,
            // If the EnrollmentModule row is somehow missing (bad seed/legacy data), the first
            // module still defaults to unlocked - it should never require "unlocking" in the
            // first place. Later modules still correctly default to locked in that scenario.
            IsUnlocked = enrollmentModule?.IsUnlocked ?? isFirstModule,
            QuizPassed = quizPassed,
            // "All lessons watched" - independent of whether the quiz exists/was passed.
            // The frontend needs THIS (not IsCompleted) to know when to trigger the
            // automatic quiz launch and to show the "waiting for quiz" badge, since
            // IsCompleted below is deliberately false until the quiz is also passed.
            AllLessonsCompleted = allLessonsCompleted,
            // Fully done: every lesson watched AND (no quiz required OR quiz passed).
            IsCompleted = allLessonsCompleted && (!hasQuiz || quizPassed),
            Lessons = lessons
        };
    }

    private static LessonDetailsDto MapLesson(Lesson lesson, Dictionary<Guid, LessonProgress> progressLookup)
    {
        progressLookup.TryGetValue(lesson.Id, out var progress);

        // Deliberately metadata-only: no SAS URL generation, no article text here.
        // The trainee only needs enough to render the lesson list (title, duration
        // via OrderIndex/progress, and a rough content-type icon) - the actual
        // content is fetched on demand via GetLessonByIdQuery when a lesson is opened.
        return new LessonDetailsDto
        {
            LessonId = lesson.Id,
            Title = lesson.Title,
            OrderIndex = lesson.OrderIndex,
            IsCompleted = progress?.IsCompleted ?? false,
            WatchedSeconds = progress?.WatchedSeconds ?? 0,
            ContentType = lesson.Type.ToString()
        };
    }
}
