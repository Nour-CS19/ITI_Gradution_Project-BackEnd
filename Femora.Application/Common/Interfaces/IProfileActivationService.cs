namespace Femora.Application.Common.Interfaces;

/// <summary>
/// Result of a lazy profile activation check. If the user did not yet have the
/// relevant profile row, it is created on the spot and fresh tokens carrying
/// that profile as the active one are issued, so the caller's very next request
/// is already authorized for it.
/// </summary>
public record ProfileActivationResult(
    Guid TraineeProfileId,
    bool WasJustActivated,
    string? Message,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt);

/// <summary>
/// Handles "lazy" creation/activation of a user's profiles. Profiles are never
/// created eagerly at registration time — every new user starts out as a plain
/// buyer. The first time the user performs an operation that belongs to a given
/// profile (e.g. enrolling in a course for Trainee), the corresponding profile
/// row is created automatically and the user is informed.
/// </summary>
public interface IProfileActivationService
{
    /// <summary>
    /// Ensures the current user has a Trainee profile, creating (and activating)
    /// it on first use if it doesn't exist yet. Call this from any LMS operation
    /// that requires a trainee to exist (e.g. enrolling in a course).
    /// </summary>
    Task<ProfileActivationResult> EnsureTraineeProfileActivatedAsync(Guid userId, CancellationToken cancellationToken = default);
}
