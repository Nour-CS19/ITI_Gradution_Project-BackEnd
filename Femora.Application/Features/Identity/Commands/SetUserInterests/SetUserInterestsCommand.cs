using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Identity.Commands.SetUserInterests;

public record SetUserInterestsCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }

    /// <summary>OnboardingInterest ids the user is interested in.</summary>
    public List<Guid> InterestIds { get; init; } = new();
}
