using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Identity.Queries.GetMyInterests;

/// <summary>
/// Returns every course/product category, each flagged with whether the current
/// user already selected it as an interest - powers the "edit my interests" screen
/// (prefills checkboxes) as well as the initial onboarding step.
/// </summary>
public record GetMyInterestsQuery : IRequest<MyInterestsResponse>
{
    public Guid UserId { get; init; }
}

public record MyInterestsResponse
{
    public List<UserInterestDto> Interests { get; init; } = new();
}

public record UserInterestDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public string? DescriptionEn { get; init; }
    public bool IsSelected { get; init; }
}
