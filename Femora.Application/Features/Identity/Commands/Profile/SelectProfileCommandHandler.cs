using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.Profile;
public class SelectProfileCommandHandler(IAuthService _authService, ICurrentUserService _currentUser) 
            : IRequestHandler<SelectProfileCommand, AuthResponseDto>
{
    async Task<AuthResponseDto> IRequestHandler<SelectProfileCommand, AuthResponseDto>.Handle(SelectProfileCommand request, CancellationToken cancellationToken)
    {
        return await _authService.SelectProfileAsync(_currentUser.UserId, request.Profile);
    }
}