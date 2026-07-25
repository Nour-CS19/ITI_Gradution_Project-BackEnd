using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Repositories.LMS;

public class  EnrollmentModuleRepository(IAppDbContext _context) : IEnrollmentModuleRepository
{
    public async Task<EnrollmentModule?> GetAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default)
    {
        return await _context.EnrollmentModules
       .Include(em => em.Module)
           .ThenInclude(m => m.Course)
       .Include(em => em.Enrollment)
       .FirstOrDefaultAsync(em => em.ModuleId == moduleId && em.EnrollmentId == enrollmentId, ct);
    }

    public async Task<EnrollmentModule?> GetByTraineeAndModuleAsync(Guid traineeProfileId, Guid moduleId, CancellationToken ct = default)
    {
        return await _context.EnrollmentModules
            .Include(em => em.Module)
                .ThenInclude(m => m.Course)
            .Include(em => em.Enrollment)
            .FirstOrDefaultAsync(em => em.ModuleId == moduleId && em.Enrollment.TraineeProfileId == traineeProfileId, ct);
    }

    public async Task UnlockAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default)
    {
        var firstEnrollmentModule = await _context.EnrollmentModules
            .FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.EnrollmentId == enrollmentId, ct)
            ?? throw new NotFoundException(nameof(EnrollmentModule), $"EnrollmentId: {enrollmentId}, ModuleId: {moduleId}");

        firstEnrollmentModule.IsUnlocked = true;
        firstEnrollmentModule.UnlockedAt = DateTime.UtcNow;
    }
}
