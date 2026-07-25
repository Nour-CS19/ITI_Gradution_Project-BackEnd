using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Repositoies.LMS;

public class ModuleRepository(IAppDbContext context) : IModuleRepository
{
    public async Task<List<Module>> GetByCourseIdOrderedAsync(Guid courseId, CancellationToken ct = default)
    {
        return await context.Modules.Where(m => m.CourseId == courseId)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync();
    }

    public async Task<Module?> GetNextModuleAsync(Guid courseId, Guid currentModuleId, CancellationToken ct = default)
    {
        var modules = await GetByCourseIdOrderedAsync(courseId, ct);
        var currentIndex = modules.FindIndex(m => m.Id == currentModuleId);

        return currentIndex == -1 || currentIndex + 1 >= modules.Count ? null : modules[currentIndex + 1];
    }
}
