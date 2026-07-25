using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Extensions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Settings;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Exceptions;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Femora.Infrastructure.Identity.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Femora.Infrastructure.Identity.Services;

public class TokenService(
                           IOptions<JwtSettings> _jwtSettings,
                           UserManager<ApplicationUser> _userManager,
                           IAppDbContext _context,
                           IProfileResolutionService _profileResolutionService
                          )
                          : ITokenService

{
    public async Task<string> GenerateAccessTokenAsync(Guid userId, ProfileType? activeProfile)
    {
        var user = await GetActiveUserAsync(userId);

        var roles = await _userManager.GetRolesAsync(user); // when admin 

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
        };

        if (activeProfile != null)
            claims.Add(new Claim(CustomClaims.Profile, activeProfile.ToString()));

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r))); // when admin 

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.SecretKey));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, ProfileType? activeProfile)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenExpiryDays),
            Token = token,
            ActiveProfile = activeProfile
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        // await _context.SaveChangesAsync();

        return token;
    }

    public async Task<SigninResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var existingToken = await ValidateRefreshTokenAsync(refreshToken);

        var user = await GetActiveUserAsync(existingToken.UserId);

        await ValidateProfileAsync(user.Id, existingToken.ActiveProfile);

        var tokens = await RotateRefreshTokenAsync(existingToken);


        return new SigninResponseDto
        {
            RequiresProfileSelection = false,
            Auth = new AuthResponseDto
            {
                User = await user.ToUserDtoAsync(_userManager),
                RefreshToken = tokens.refreshToken,
                AccessToken = tokens.accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes),
                ActiveProfile = existingToken.ActiveProfile
            },
        };

    }

    public async Task RevokeTokenAsync(Guid userId, string refreshToken)
    {
        var token = await ValidateRefreshTokenAsync(refreshToken);

        if (token.UserId != userId)
            throw new InvalidTokenException("Token does not belong to user");

        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    private async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new InvalidTokenException("Refresh token cannot be empty");

        var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (existingToken is null)
            throw new InvalidTokenException("Invalid or expired refresh token");

        if (existingToken.IsExpired)
            throw new InvalidTokenException("Refresh token has expired");

        if (existingToken.IsRevoked)
            throw new InvalidTokenException("Refresh token has been revoked");

        return existingToken;
    }
    private async Task ValidateProfileAsync(Guid userId, ProfileType? activeProfile)
    {
        if (activeProfile is null)
            return;

        var availableProfiles = await _profileResolutionService.GetAvailableProfilesAsync(userId);

        if (!availableProfiles.Contains(activeProfile.Value))
            throw new ProfileNoLongerAvailableException("Profile no longer available.");

    }
    private async Task<ApplicationUser> GetActiveUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
             ?? throw new NotFoundException(nameof(ApplicationUser), userId.ToString());


        if (!user.IsActive)
            throw new AuthenticationException("Account is deactivated");

        return user;
    }
    private async Task<(string accessToken, string refreshToken)> RotateRefreshTokenAsync(RefreshToken existingToken)
    {
        await using var transaction = await _context.BeginTransactionAsync();

        try
        {
            existingToken.IsRevoked = true;
            _context.RefreshTokens.Update(existingToken);

            var accessToken = await GenerateAccessTokenAsync(existingToken.UserId, existingToken.ActiveProfile);
            var refreshToken = await GenerateRefreshTokenAsync(existingToken.UserId, existingToken.ActiveProfile);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (accessToken, refreshToken);
        }

        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
