using System.Security.Claims;
using Femora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Femora.API.Authorization
{
    public class SellerAuthorizationHandler(IAppDbContext dbContext) : AuthorizationHandler<SellerRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SellerRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return;

            // Allow Admin users to pass the Seller policy (admins may manage marketplace data)
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Query database directly to bypass any stale JWT claims
            var hasProfile = await dbContext.SellerProfiles.AnyAsync(sp => sp.UserId == userId);
            if (hasProfile)
            {
                context.Succeed(requirement);
            }
        }
    }
}
