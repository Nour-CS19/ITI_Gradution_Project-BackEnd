using System.Security.Claims;
using Femora.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Femora.Infrastructure.Identity.Services;

public class CurrentUserService(IHttpContextAccessor _httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User is not authenticated");

            return Guid.Parse(userId);
        }
    }
}