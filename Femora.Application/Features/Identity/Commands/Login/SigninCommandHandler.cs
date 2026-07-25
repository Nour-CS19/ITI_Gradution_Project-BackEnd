using AutoMapper;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Requests;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.Login;
public class SigninCommandHandler(IAuthService authService, IMapper mapper)
    : IRequestHandler<SigninCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(SigninCommand request, CancellationToken cancellationToken)
    {
        var signinRequest = mapper.Map<SigninRequest>(request);
        return await authService.SigninAsync(signinRequest);
    }
}
