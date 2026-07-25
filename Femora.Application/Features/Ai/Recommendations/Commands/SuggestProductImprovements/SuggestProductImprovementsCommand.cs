using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.AI.Recommendations.Commands.SuggestProductImprovements;

/// <summary>
/// Generates textual improvement suggestions for a seller's product listing
/// (description quality, missing images, title clarity, etc.) using AI.
/// </summary>
public record SuggestProductImprovementsCommand : IRequest<SuggestProductImprovementsResponse>
{
    public Guid ProductId { get; init; }
}

public record SuggestProductImprovementsResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public string OverallAssessment { get; init; } = string.Empty;
}
