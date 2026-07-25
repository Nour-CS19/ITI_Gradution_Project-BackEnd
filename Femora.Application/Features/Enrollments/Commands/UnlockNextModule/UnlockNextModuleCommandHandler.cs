using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Module = Femora.Domain.Entities.LMS.Module;

namespace Femora.Application.Features.Enrollments.Commands.UnlockNextModule;

public class UnlockNextModuleCommandHandler(
        IAppDbContext _context,
        IModuleRepository _moduleRepo,
        IQuizRepository _quizRepo,
        IEnrollmentModuleRepository _enrollmentModuleRepo,
        ICurrentUserService _currentUser)
        : IRequestHandler<UnlockNextModuleCommand, UnlockNextModuleResponse>
{
    public async Task<UnlockNextModuleResponse> Handle(UnlockNextModuleCommand request, CancellationToken cancellationToken)
    {
        var traineeProfile = await _context.TraineeProfiles
          .FirstOrDefaultAsync(tp => tp.UserId == _currentUser.UserId, cancellationToken)
          ?? throw new NotFoundException(nameof(TraineeProfile), _currentUser.ToString());

        var currentEnrollmentModule = await _enrollmentModuleRepo
                                .GetByTraineeAndModuleAsync(traineeProfile.Id, request.ModuleId, cancellationToken)
                                ?? throw new NotFoundException(nameof(EnrollmentModule), $"TraineeProfileId: {traineeProfile.Id}, ModuleId: {request.ModuleId}");

        var enrollment = currentEnrollmentModule.Enrollment;

        var hasPassed = await _quizRepo.HasPassedAsync(enrollment.Id, request.ModuleId, cancellationToken);

        if (!hasPassed)
            throw new QuizNotPassedException(request.ModuleId);

        var nextModule = await _moduleRepo.GetNextModuleAsync(enrollment.CourseId, request.ModuleId, cancellationToken);

        if (nextModule is null)
            return new UnlockNextModuleResponse { IsLastModule = true };

        var nextEnrollmentModule = await _context.EnrollmentModules
                                .FirstOrDefaultAsync(em => em.EnrollmentId == enrollment.Id &&
                                em.ModuleId == nextModule.Id, cancellationToken)
                            ?? throw new NotFoundException(nameof(EnrollmentModule), nextModule.Id.ToString());


        if (nextEnrollmentModule.IsUnlocked)
            return BuildResponse(nextModule, alreadyUnlocked: true, isLastModule: false);

        nextEnrollmentModule.IsUnlocked = true;
        await _context.SaveChangesAsync(cancellationToken);

        return BuildResponse(nextModule, isLastModule: false, alreadyUnlocked: false);
    }

    private static UnlockNextModuleResponse BuildResponse(Module module, bool isLastModule, bool alreadyUnlocked)
        => new UnlockNextModuleResponse()
        {
            UnlockedModuleId = module.Id,
            UnlockedModuleTitle = module.Title,
            ModuleOrderIndex = module.OrderIndex,
            IsLastModule = isLastModule,
            AlreadyUnlocked = alreadyUnlocked
        };
}
