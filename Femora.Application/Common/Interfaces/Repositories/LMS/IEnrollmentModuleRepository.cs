using Femora.Domain.Entities.LMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.LMS;
public interface IEnrollmentModuleRepository
{
    Task UnlockAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default);
    Task<EnrollmentModule?> GetByTraineeAndModuleAsync(Guid traineeProfileId,  Guid moduleId, CancellationToken ct = default);
    Task<EnrollmentModule?> GetAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default);
}
