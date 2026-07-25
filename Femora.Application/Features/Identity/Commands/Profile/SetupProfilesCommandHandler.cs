using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.Profile;

public class SetupProfilesCommandHandler(IAuthService authService, ICurrentUserService currentUser)
    : IRequestHandler<SetupProfilesCommand, SigninResponseDto>
{
    public Task<SigninResponseDto> Handle(SetupProfilesCommand request, CancellationToken cancellationToken)
        => authService.SetupProfilesAsync(currentUser.UserId, request.Roles, cancellationToken);
}
