using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Femora.Application.Common.Extensions;
public static class UserMappingExtensions
{
    public static async Task<UserDTO> ToUserDtoAsync(this ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        return new UserDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            Role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
        };
    }
}
