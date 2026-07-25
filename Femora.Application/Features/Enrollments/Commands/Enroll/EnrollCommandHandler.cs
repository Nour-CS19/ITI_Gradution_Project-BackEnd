using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Enrollments.Commands.Enroll;

public class EnrollCommandHandler(
    IAppDbContext _context,
    IEnrollmentRepository _enrollmentRepo,
    ICurrentUserService _currentUser,
    IProfileActivationService _profileActivation)
    : IRequestHandler<EnrollCommand, EnrollmentResponse>
{
    public async Task<EnrollmentResponse> Handle(EnrollCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var course = await GetValidatedCourseAsync(request.CourseId, cancellationToken);

        // Paid courses must go through Stripe Checkout (POST /api/payments/checkout with
        // courseId). The enrollment itself is then created by the webhook handler
        // (StripeService.FulfillCourseEnrollmentAsync) once payment is confirmed.
        // This endpoint only creates enrollments directly for free courses — check this
        // BEFORE touching the trainee profile so a blocked/failed attempt never leaves
        // a profile behind for a user who didn't actually enroll in anything.
        if (course.Price > 0)
            throw new PaymentRequiredException();

        var alreadyEnrolled = await _context.Enrollments.AnyAsync(
            e => e.CourseId == course.Id && e.TraineeProfile.UserId == userId,
            cancellationToken);

        if (alreadyEnrolled)
            throw new AlreadyEnrolledException(request.CourseId);

        using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            // Everything checks out — this is a real enrollment, so this is the moment a
            // plain buyer's Trainee profile is lazily activated (if it doesn't exist yet).
            var activation = await _profileActivation.EnsureTraineeProfileActivatedAsync(userId, cancellationToken);

            var enrollment = await _enrollmentRepo
                            .EnrollAsync(activation.TraineeProfileId, request.CourseId, course.Price, cancellationToken);

            var lessonProgresses = CreateLessonProgresses(enrollment, course);
            await _context.LessonProgresses.AddRangeAsync(lessonProgresses, cancellationToken);

            var enrollmentModules = course.Modules.Select(m => new EnrollmentModule
            {
                ModuleId = m.Id,
                EnrollmentId = enrollment.Id,
                IsUnlocked = false,
            }).ToList();

            await _context.EnrollmentModules.AddRangeAsync(enrollmentModules, cancellationToken);

            var firstModule = course.Modules.OrderBy(m => m.OrderIndex).FirstOrDefault();
            UnlockFirstModule(firstModule, enrollmentModules);

            var instructorEarning = CreateInstructorEarning(enrollment, course);
            await _context.InstructorEarnings.AddAsync(instructorEarning, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new EnrollmentResponse
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                EnrolledAt = enrollment.EnrolledAt,
                PricePaid = enrollment.PricePaid,
                FirstModuleId = firstModule?.Id,
                Status = EnrollmentStatus.Active,
                TraineeProfileActivated = activation.WasJustActivated,
                ActivationMessage = activation.Message,
                AccessToken = activation.AccessToken,
                RefreshToken = activation.RefreshToken,
                ExpiresAt = activation.ExpiresAt
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Course> GetValidatedCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .Include(c => c.Modules).ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);

        if (course is null)
            throw new NotFoundException(nameof(Course), courseId.ToString());

        if (!course.IsPublished)
            throw new CourseNotPublishedException(courseId);

        return course;
    }
    private static List<LessonProgress> CreateLessonProgresses(Enrollment enrollment, Course course)
    {
        return course.Modules
            .SelectMany(m => m.Lessons)
            .Select(lesson => new LessonProgress
            {
                EnrollmentId = enrollment.Id,
                LessonId = lesson.Id,
                IsCompleted = false,
                WatchedSeconds = 0,
                LastAccessedAt = null
            }).ToList();
    }
    private static InstructorEarning CreateInstructorEarning(Enrollment enrollment, Course course)
    {
        return new InstructorEarning
        {
            InstructorProfileId = course.InstructorProfileId,
            EnrollmentId = enrollment.Id,
            Amount = enrollment.PricePaid,
            PlatformFee = InstructorEarning.CalculatePlatformFee(enrollment.PricePaid),
            Status = EarningStatus.Pending,
            EarnedAt = DateTime.UtcNow
        };
    }
    private static void UnlockFirstModule(Module? firstModule, List<EnrollmentModule> enrollmentModules)
    {
        if (firstModule is null)
            return;

        var firstEnrollmentModule = enrollmentModules
        .First(x => x.ModuleId == firstModule.Id);

        firstEnrollmentModule.Unlock();
    }
}
