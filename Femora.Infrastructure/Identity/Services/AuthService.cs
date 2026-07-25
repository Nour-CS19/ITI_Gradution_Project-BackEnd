using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Extensions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Settings;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Exceptions;
using Femora.Application.Features.Identity.Common.Requests;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Femora.Infrastructure.Identity.Services;

public class AuthService(UserManager<ApplicationUser> _userManager,
                         IAppDbContext _context,
                         ITokenService _tokenService,
                         IOptions<JwtSettings> _jwtSettings,
                         IProfileResolutionService _profileResolutionService)
                        : IAuthService
{
    public async Task LogoutAsync(Guid userId, string refreshToken) => await _tokenService.RevokeTokenAsync(userId, refreshToken);
    public async Task<SigninResponseDto> RegisterAsync(RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            throw new EmailAlreadyExistsException(request.Email);

        await ValidateOnboardingAsync(request);

        var user = CreateUser(request);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new RegistrationFailedException(string.Join(", ", result.Errors.Select(r => r.Description)));

        var interests = await _context.OnboardingInterests
            .Where(i => request.InterestIds.Contains(i.Id) && i.IsActive)
            .ToListAsync();

        foreach (var interest in interests)
        {
            user.OnboardingInterests.Add(interest);
        }

        var auth = new AuthResponseDto
        {
            User = await user.ToUserDtoAsync(_userManager),
            AccessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, null),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, null),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes)
        };

        await _context.SaveChangesAsync();

        return new SigninResponseDto { RequiresProfileSelection = false, Auth = auth };
    }
    public async Task<SigninResponseDto> SigninAsync(SigninRequest request)
    {
        var user = await ValidateUserCredentialsAsync(request);

        var profiles = await _profileResolutionService.GetAvailableProfilesAsync(user.Id);

        if (!profiles.Any())
        {
            return new SigninResponseDto
            {
                RequiresProfileSelection = false,
                Auth = await BuildAuthResponseAsync(user, null)
            };
        }

        if (profiles.Count == 1)
        {
            return new SigninResponseDto
            {
                RequiresProfileSelection = false,
                Auth = await BuildAuthResponseAsync(user, profiles.First())
            };
        }

        return new SigninResponseDto
        {
            RequiresProfileSelection = true,
            Auth = await BuildAuthResponseAsync(user, null),
            AvailableProfiles = profiles.Select(p => p.ToDto()).ToList()
        };
    }
    public async Task<SigninResponseDto> SetupProfilesAsync(Guid userId, List<ProfileType> roles, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId.ToString());

        // Create each requested profile (idempotent — skip if already exists)
        foreach (var role in roles.Distinct())
        {
            switch (role)
            {
                case ProfileType.Trainee:
                    // Trainee profiles are created only after a confirmed course enrollment.
                    // Registration/setup keeps the user as a plain ApplicationUser.
                    break;

                case ProfileType.Instructor:
                    if (!await _context.InstructorProfiles.AnyAsync(i => i.UserId == userId, cancellationToken))
                        _context.InstructorProfiles.Add(new InstructorProfile { UserId = userId, Status = VerificationStatus.Pending });
                    break;

                case ProfileType.Seller:
                    if (!await _context.SellerProfiles.AnyAsync(s => s.UserId == userId, cancellationToken))
                        _context.SellerProfiles.Add(new SellerProfile { UserId = userId, Status = VerificationStatus.Pending });
                    break;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Resolve which profiles are now available (Trainee only — Instructor/Seller need approval)
        var availableProfiles = await _profileResolutionService.GetAvailableProfilesAsync(userId, cancellationToken);

        // Single available profile → auto-select it
        if (availableProfiles.Count == 1)
        {
            return new SigninResponseDto
            {
                RequiresProfileSelection = false,
                Auth = await BuildAuthResponseAsync(user, availableProfiles.First())
            };
        }

        // Multiple available profiles → let user pick (only happens if trainee + approved instructor/seller)
        if (availableProfiles.Count > 1)
        {
            return new SigninResponseDto
            {
                RequiresProfileSelection = true,
                AvailableProfiles = availableProfiles.Select(p => p.ToDto()).ToList(),
                Auth = await BuildAuthResponseAsync(user, null)
            };
        }

        // No available profiles yet (pending approval for instructor/seller only)
        // Return token with no active profile — frontend will show "pending approval" message
        return new SigninResponseDto
        {
            RequiresProfileSelection = false,
            Auth = await BuildAuthResponseAsync(user, null)
        };
    }

    public async Task<AuthResponseDto> SelectProfileAsync(Guid userId, ProfileType profile)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId.ToString());

        var availableProfiles = await _profileResolutionService.GetAvailableProfilesAsync(userId);

        if (!availableProfiles.Contains(profile))
            throw new InvalidProfileSelectionException();


        return await BuildAuthResponseAsync(user, profile);
    }
    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, ProfileType? activeProfile)
    {
        var auth = new AuthResponseDto
        {
            User = await user.ToUserDtoAsync(_userManager),
            AccessToken = await _tokenService.GenerateAccessTokenAsync(user.Id, activeProfile),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, activeProfile),
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.Value.ExpiryMinutes),
            ActiveProfile = activeProfile
        };
        await _context.SaveChangesAsync();

        return auth;
    }
    private async Task<ApplicationUser> ValidateUserCredentialsAsync(SigninRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
          ?? throw new AuthenticationException("Invalid credentials");

        if (!user.IsActive)
            throw new AuthenticationException("Account is deactivated");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new AuthenticationException("Invalid credentials");

        return user;
    }
    private async Task ValidateOnboardingAsync(RegisterRequest request)
    {
        var goalExists = await _context.OnboardingGoals
            .AnyAsync(g => g.Id == request.OnboardingGoalId && g.IsActive);

        if (!goalExists)
            throw new NotFoundException("OnboardingGoal", request.OnboardingGoalId.ToString());

        var interestIds = (request.InterestIds ?? new List<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (interestIds.Count == 0)
            throw new RegistrationFailedException("At least one interest is required.");

        var existingCount = await _context.OnboardingInterests
            .CountAsync(i => interestIds.Contains(i.Id) && i.IsActive);

        if (existingCount != interestIds.Count)
            throw new NotFoundException("OnboardingInterest", "One or more selected interests were not found or are inactive.");
    }

    private static ApplicationUser CreateUser(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            OnboardingGoalId = request.OnboardingGoalId,
        };

        return user;
    }
}
