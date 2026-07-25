using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Requests;
using Femora.Domain.Enums;

namespace Femora.Application.Common.Interfaces;
public interface IAuthService
{
    Task<SigninResponseDto> RegisterAsync(RegisterRequest request);
    Task<SigninResponseDto> SigninAsync(SigninRequest request);
    Task LogoutAsync(Guid userId, string refreshToken);
    Task<AuthResponseDto> SelectProfileAsync(Guid userId, ProfileType profile);
    Task<SigninResponseDto> SetupProfilesAsync(Guid userId, List<ProfileType> roles, CancellationToken cancellationToken = default);
}
