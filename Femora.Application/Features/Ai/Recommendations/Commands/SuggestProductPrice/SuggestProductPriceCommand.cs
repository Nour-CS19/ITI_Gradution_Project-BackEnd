using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.AI.Recommendations.Commands.SuggestProductPrice;

/// <summary>
/// Suggests a price for a seller's product by combining market data
/// (similar products in the same category, price range, avg price)
/// with an AI reasoning pass over that data.
/// </summary>
public record SuggestProductPriceCommand : IRequest<SuggestProductPriceResponse>
{
    public Guid ProductId { get; init; }
}

public record SuggestProductPriceResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal? CurrentPrice { get; init; }
    public decimal SuggestedMinPrice { get; init; }
    public decimal SuggestedMaxPrice { get; init; }
    public decimal SuggestedPrice { get; init; }
    public string Reasoning { get; init; } = string.Empty;
    public MarketDataDto MarketData { get; init; } = new();
}

public record MarketDataDto
{
    public int SimilarProductsCount { get; init; }
    public decimal? MinObservedPrice { get; init; }
    public decimal? MaxObservedPrice { get; init; }
    public decimal? AverageObservedPrice { get; init; }
}
