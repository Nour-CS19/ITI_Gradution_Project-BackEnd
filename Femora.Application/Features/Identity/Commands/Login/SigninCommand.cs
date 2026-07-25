using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.Login;
public sealed record SigninCommand(string Email, string Password) : IRequest<SigninResponseDto>;
