using Femora.Domain.Entities.LMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.LMS;

public interface IModuleRepository
{
    Task<List<Module>> GetByCourseIdOrderedAsync(Guid courseId, CancellationToken ct = default);
    Task<Module?> GetNextModuleAsync(Guid courseId, Guid currentModuleId, CancellationToken ct = default);
}
