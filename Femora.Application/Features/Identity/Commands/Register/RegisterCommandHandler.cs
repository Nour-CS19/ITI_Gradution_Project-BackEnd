using AutoMapper;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Requests;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.Register;
public class RegisterCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<RegisterCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = mapper.Map<RegisterRequest>(request);
        return await authService.RegisterAsync(registerRequest);
    }
}
