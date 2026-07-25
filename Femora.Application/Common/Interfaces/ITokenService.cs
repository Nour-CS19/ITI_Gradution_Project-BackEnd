using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(Guid userId, ProfileType? activeProfile);
    Task<string> GenerateRefreshTokenAsync(Guid userId, ProfileType? activeProfile);
    Task<SigninResponseDto> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(Guid userId, string refreshToken);
}
