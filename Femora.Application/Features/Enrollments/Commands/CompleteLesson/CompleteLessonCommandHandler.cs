using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.LMS.Quizzes.Commands.GenerateQuiz;
using Femora.Domain.Entities.LMS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Enrollments.Commands.CompleteLesson;

public class CompleteLessonCommandHandler(
    IAppDbContext _context,
    ICurrentUserService _currentUser,
    IQuizRepository _quizRepo,
    IMediator _mediator)
    : IRequestHandler<CompleteLessonCommand, CompleteLessonResponse>
{
    public async Task<CompleteLessonResponse> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
            ?? throw new NotFoundException(nameof(Lesson), request.LessonId.ToString());

        var enrollment = await _context.Enrollments
            .Include(e => e.LessonProgresses)
            .FirstOrDefaultAsync(
                e => e.CourseId == lesson.Module.CourseId && e.TraineeProfile.UserId == _currentUser.UserId,
                cancellationToken)
            ?? throw new ForbiddenException("You are not enrolled in the course this lesson belongs to.");

        // Server-side lock enforcement: a module stays locked until the previous module's
        // quiz is passed (see UnlockNextModuleCommandHandler). The frontend already hides
        // locked lessons, but that's UI-only - without this check a trainee could still call
        // this endpoint directly for a lesson in a module they haven't unlocked yet and mark
        // it complete without ever taking/passing the previous quiz.
        var enrollmentModule = await _context.EnrollmentModules
            .FirstOrDefaultAsync(em => em.EnrollmentId == enrollment.Id && em.ModuleId == lesson.ModuleId, cancellationToken);

        if (enrollmentModule is not null && !enrollmentModule.IsUnlocked)
            throw new ForbiddenException("This module is locked. Pass the previous module's quiz to unlock it first.");

        // The trainee may not have a LessonProgress row for this lesson yet - this happens
        // whenever progress wasn't (or couldn't be) pre-seeded for every lesson at enrollment
        // time (e.g. demo/seed data, or a lesson added to the course after enrolling).
        // Rather than 404-ing on the trainee for something that isn't their fault, create the
        // row on demand so completing a lesson always works as long as they're really enrolled.
        var progress = enrollment.LessonProgresses.FirstOrDefault(lp => lp.LessonId == request.LessonId);
        if (progress is null)
        {
            progress = new LessonProgress
            {
                EnrollmentId = enrollment.Id,
                LessonId = request.LessonId,
                IsCompleted = false,
                WatchedSeconds = 0,
            };
            await _context.LessonProgresses.AddAsync(progress, cancellationToken);
            enrollment.LessonProgresses.Add(progress);
        }

        if (!progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.LastAccessedAt = DateTime.UtcNow;
        }

        var moduleId = lesson.ModuleId;

        // Is this the last (highest OrderIndex) lesson in its module? If so, the trainee
        // needs to take the module quiz next - generating it on the spot if it doesn't
        // exist yet, so the frontend always has a quiz id to redirect to immediately.
        var maxOrderIndexInModule = await _context.Lessons
            .Where(l => l.ModuleId == moduleId)
            .MaxAsync(l => l.OrderIndex, cancellationToken);

        var isLastLessonInModule = lesson.OrderIndex == maxOrderIndexInModule;

        Guid? moduleQuizId = null;
        if (isLastLessonInModule)
        {
            var existingQuiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.ModuleId == moduleId, cancellationToken);

            if (existingQuiz is not null)
            {
                moduleQuizId = existingQuiz.Id;
            }
            else
            {
                try
                {
                    var generated = await _mediator.Send(new GenerateQuizCommand { ModuleId = moduleId }, cancellationToken);
                    moduleQuizId = generated.QuizId;
                }
                catch (Exception)
                {
                    // Quiz generation is best-effort here (it depends on external AI/search
                    // services). If it fails, the lesson completion itself must still be
                    // saved - the frontend already re-requests quiz generation when the
                    // trainee opens the module, and that request will retry this same
                    // operation on demand instead of ever swallowing this silently.
                    moduleQuizId = null;
                }
            }
        }

        var allLessonsCompleted = enrollment.LessonProgresses.All(lp => lp.IsCompleted);

        bool allModulesPassed = true;

        if (allLessonsCompleted)
        {
            var modules = await _context.Modules
                .Where(m => m.CourseId == enrollment.CourseId)
                .ToListAsync(cancellationToken);

            foreach (var module in modules)
            {
                // A module "has a quiz" if one exists for it, regardless of the (unreliable)
                // Module.QuizId pointer - the quiz itself always carries its ModuleId.
                var hasQuiz = await _context.Quizzes.AnyAsync(q => q.ModuleId == module.Id, cancellationToken);
                if (hasQuiz)
                {
                    var passed = await _quizRepo.HasPassedAsync(enrollment.Id, module.Id, cancellationToken);
                    if (!passed)
                    {
                        allModulesPassed = false;
                        break;
                    }
                }
            }
        }

        if (allLessonsCompleted && allModulesPassed)
        {
            enrollment.IsCompleted = true;
            enrollment.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CompleteLessonResponse
        {
            LessonId = request.LessonId,
            EnrollmentId = enrollment.Id,
            EnrollmentCompleted = enrollment.IsCompleted,
            EnrollmentCompletedAt = enrollment.CompletedAt,
            IsLastLessonInModule = isLastLessonInModule,
            ModuleId = moduleId,
            ModuleQuizId = moduleQuizId
        };
    }
}