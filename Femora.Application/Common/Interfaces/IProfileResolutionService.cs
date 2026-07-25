using Femora.Domain.Enums;

namespace Femora.Application.Common.Interfaces;

public interface IProfileResolutionService
{
    Task<IReadOnlyCollection<ProfileType>> GetAvailableProfilesAsync(Guid userId, CancellationToken cancellationToken = default);
}
