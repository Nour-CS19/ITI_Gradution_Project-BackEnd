using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Extensions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Femora.Application.Features.Identity.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(
    UserManager<ApplicationUser> _userManager,
    ITokenService _tokenService)
    : IRequestHandler<VerifyEmailCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.EmailConfirmed)
            // Already verified – just return a fresh session so the client can proceed
            return await BuildResponseAsync(user);

        // The token from the URL has been Base64-URL encoded by the controller; decode it
        var decodedToken = System.Web.HttpUtility.UrlDecode(request.Token);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Email verification failed: {errors}");
        }

        return await BuildResponseAsync(user);
    }

    private async Task<SigninResponseDto> BuildResponseAsync(ApplicationUser user)
    {
        var auth = new AuthResponseDto
        {
            User = await user.ToUserDtoAsync(_userManager),
            AccessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, null),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, null),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        return new SigninResponseDto
        {
            RequiresProfileSelection = false,
            Auth = auth
        };
    }
}
