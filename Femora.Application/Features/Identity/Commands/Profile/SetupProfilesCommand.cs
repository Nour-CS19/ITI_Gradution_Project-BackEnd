using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Enums;
using MediatR;

namespace Femora.Application.Features.Identity.Commands.Profile;

public record SetupProfilesCommand(List<ProfileType> Roles) : IRequest<SigninResponseDto>;
