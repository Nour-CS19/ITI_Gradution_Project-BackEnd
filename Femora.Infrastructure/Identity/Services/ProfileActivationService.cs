using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Settings;
using Femora.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Femora.Infrastructure.Identity.Services;

public class ProfileActivationService(
    IAppDbContext _context,
    IOnboardingProfileSyncService _onboardingProfileSync,
    ITokenService _tokenService,
    IOptions<JwtSettings> _jwtSettings) : IProfileActivationService
{
    public async Task<ProfileActivationResult> EnsureTraineeProfileActivatedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _onboardingProfileSync.EnsureTraineeProfileAsync(userId, cancellationToken);

        if (!result.WasCreated)
            return new ProfileActivationResult(result.TraineeProfileId, false, null, null, null, null);

        var accessToken = await _tokenService.GenerateAccessTokenAsync(userId, ProfileType.Trainee);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, ProfileType.Trainee);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes);

        await _context.SaveChangesAsync(cancellationToken);

        return new ProfileActivationResult(
            TraineeProfileId: result.TraineeProfileId,
            WasJustActivated: true,
            Message: "تم تفعيل ملفك كمتدربة بعد التسجيل في الدورة",
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: expiresAt);
    }
}
