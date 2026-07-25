using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Identity.Services;

public class ProfileResolutionService(IAppDbContext context) : IProfileResolutionService
{
    public async Task<IReadOnlyCollection<ProfileType>> GetAvailableProfilesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Shared here because sign-in, select-portal, and refresh all need the same profile approval rules.
        var profiles = new List<ProfileType>();

        if (await context.TraineeProfiles.AnyAsync(trainee => trainee.UserId == userId))
            profiles.Add(ProfileType.Trainee);

        if (await context.InstructorProfiles.AnyAsync(
                profile => profile.UserId == userId && profile.Status == VerificationStatus.Approved,
                cancellationToken))
        {
            profiles.Add(ProfileType.Instructor);
        }

        if (await context.SellerProfiles.AnyAsync(
                profile => profile.UserId == userId && profile.Status == VerificationStatus.Approved,
                cancellationToken))
        {
            profiles.Add(ProfileType.Seller);
        }

        return profiles;
    }
}
