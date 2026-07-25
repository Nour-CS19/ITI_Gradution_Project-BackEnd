using Femora.Domain.Enums;

namespace Femora.Application.Features.Identity.Common.Requests;

public record SetupProfilesRequest(List<ProfileType> Roles);
