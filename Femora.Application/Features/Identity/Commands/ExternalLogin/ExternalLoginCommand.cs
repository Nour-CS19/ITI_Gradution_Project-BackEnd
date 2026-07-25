using Femora.Application.Common.Extensions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.ExternalAuth;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Femora.Application.Features.Identity.Commands.ExternalLogin;

/// <summary>
/// Exchanges an OAuth id_token (Google) or access_token (Facebook)
/// for a Femora JWT session.
/// Provider: "Google" | "Facebook"
/// </summary>
public sealed record ExternalLoginCommand(
    string Provider,
    string IdToken) : IRequest<SigninResponseDto>;

public class ExternalLoginCommandHandler(
    UserManager<ApplicationUser> _userManager,
    ITokenService _tokenService,
    IExternalAuthRepository _externalAuthRepository)
    : IRequestHandler<ExternalLoginCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(
        ExternalLoginCommand request,
        CancellationToken cancellationToken)
    {
        var externalUser = await _externalAuthRepository.ValidateTokenAsync(
            request.Provider, request.IdToken, cancellationToken);

        var user = await FindOrCreateUserAsync(externalUser, request.Provider);

        var auth = new AuthResponseDto
        {
            User         = await user.ToUserDtoAsync(_userManager),
            AccessToken  = await _tokenService.GenerateAccessTokenAsync(user.Id, null),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, null),
            ExpiresAt    = DateTime.UtcNow.AddHours(1),
        };

        return new SigninResponseDto { RequiresProfileSelection = false, Auth = auth };
    }

    private async Task<ApplicationUser> FindOrCreateUserAsync(ExternalUserInfo externalUser, string provider)
    {
        var user = await _userManager.FindByLoginAsync(provider, externalUser.ProviderKey);
        if (user is not null) return user;

        user = await _userManager.FindByEmailAsync(externalUser.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                FirstName      = externalUser.FirstName,
                LastName       = externalUser.LastName,
                Email          = externalUser.Email,
                UserName       = externalUser.Email,
                AvatarUrl      = externalUser.PictureUrl,
                IsActive       = true,
                EmailConfirmed = true,
                CreatedAt      = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user from {provider} login: {errors}");
            }
        }

        await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, externalUser.ProviderKey, provider));
        return user;
    }
}
