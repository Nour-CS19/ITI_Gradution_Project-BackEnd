using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.RefreshToken;
public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<SigninResponseDto>;

