namespace Femora.Application.Common.Interfaces;

public record TraineeProfileSyncResult(Guid TraineeProfileId, bool WasCreated);

public interface IOnboardingProfileSyncService
{
    Task<TraineeProfileSyncResult> EnsureTraineeProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
