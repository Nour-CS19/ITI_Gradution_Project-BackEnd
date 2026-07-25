using System.Security.Claims;
using Femora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Femora.API.Authorization
{
    public class TraineeAuthorizationHandler(IAppDbContext dbContext) : AuthorizationHandler<TraineeRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TraineeRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return;

            // Query database directly to bypass any stale JWT claims
            var hasProfile = await dbContext.TraineeProfiles.AnyAsync(tp => tp.UserId == userId);
            if (hasProfile)
            {
                context.Succeed(requirement);
            }
        }
    }
}
