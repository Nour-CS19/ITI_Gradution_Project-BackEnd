using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.RefreshToken;

public class RefreshTokenCommandHandler(ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await tokenService.RefreshTokenAsync(request.RefreshToken);
    }
}
