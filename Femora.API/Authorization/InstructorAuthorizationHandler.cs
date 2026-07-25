using System.Security.Claims;
using Femora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Femora.API.Authorization
{
    public class InstructorAuthorizationHandler(IAppDbContext dbContext) : AuthorizationHandler<InstructorRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            InstructorRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return;

            // Query database directly to bypass any stale JWT claims
            var hasProfile = await dbContext.InstructorProfiles.AnyAsync(ip => ip.UserId == userId);
            if (hasProfile)
            {
                context.Succeed(requirement);
            }
        }
    }
}
