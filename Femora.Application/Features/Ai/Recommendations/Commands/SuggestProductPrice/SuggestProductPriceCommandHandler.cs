using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Entities.AI;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.AI.Recommendations.Commands.SuggestProductPrice;

public class SuggestProductPriceCommandHandler(
    IAppDbContext db,
    IChatCompletionRepository chatCompletionRepository)
    : IRequestHandler<SuggestProductPriceCommand, SuggestProductPriceResponse>
{
    public async Task<SuggestProductPriceResponse> Handle(SuggestProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductCategory)
            .Include(p => p.SellerProfile)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId.ToString());

        // Market data: prices of other published products in the same category.
        var similarPrices = await db.Products
            .Where(p => p.ProductCategoryId == product.ProductCategoryId
                        && p.Id != product.Id
                        && p.IsPuplished)
            .SelectMany(p => p.ProductVariants.Select(v => v.Price))
            .ToListAsync(cancellationToken);

        var marketData = new MarketDataDto
        {
            SimilarProductsCount = similarPrices.Count,
            MinObservedPrice = similarPrices.Count > 0 ? similarPrices.Min() : null,
            MaxObservedPrice = similarPrices.Count > 0 ? similarPrices.Max() : null,
            AverageObservedPrice = similarPrices.Count > 0 ? Math.Round(similarPrices.Average(), 2) : null
        };

        var currentPrice = product.ProductVariants.Select(v => (decimal?)v.Price).FirstOrDefault();

        var (min, max, suggested, reasoning) = await AskAiForPriceAsync(
            product.Name,
            product.ProductCategory?.Name ?? "Unknown",
            currentPrice,
            marketData,
            cancellationToken);

        // Persist as an AI recommendation for the seller to review/track.
        db.AIRecommendations.Add(new AIRecommendation
        {
            UserId = product.SellerProfile?.UserId ?? Guid.Empty,
            Type = AIRecommendationType.Product,
            EntityId = product.Id,
            EntityType = "ProductPriceSuggestion",
            Score = 1.0,
            ReasonJson = JsonSerializer.Serialize(new
            {
                suggested,
                min,
                max,
                reasoning,
                marketData
            }),
            GeneratedAt = DateTime.UtcNow
        });

        if (product.SellerProfile is not null)
            await db.SaveChangesAsync(cancellationToken);

        return new SuggestProductPriceResponse
        {
            ProductId = product.Id,
            ProductName = product.Name,
            CurrentPrice = currentPrice,
            SuggestedMinPrice = min,
            SuggestedMaxPrice = max,
            SuggestedPrice = suggested,
            Reasoning = reasoning,
            MarketData = marketData
        };
    }

    private async Task<(decimal Min, decimal Max, decimal Suggested, string Reasoning)> AskAiForPriceAsync(
        string productName,
        string categoryName,
        decimal? currentPrice,
        MarketDataDto marketData,
        CancellationToken cancellationToken)
    {
        // No market data at all - fall back to a simple heuristic instead of hallucinating numbers.
        if (marketData.SimilarProductsCount == 0)
        {
            var fallback = currentPrice ?? 0m;
            return (fallback, fallback, fallback,
                "No similar products found in this category yet to benchmark against. Keeping the current price unchanged.");
        }

        var systemPrompt =
            "You are a pricing analyst for an e-commerce marketplace. " +
            "Respond ONLY with a valid JSON object, no markdown, no explanation outside the JSON. " +
            "Schema: { \"minPrice\": number, \"maxPrice\": number, \"suggestedPrice\": number, \"reasoning\": string }. " +
            "Base your suggestion strictly on the market data provided. " +
            "The reasoning should be 2-3 short sentences, written for a seller (not technical).";

        var userPrompt =
            $"Product: {productName}\n" +
            $"Category: {categoryName}\n" +
            $"Current price: {(currentPrice.HasValue ? currentPrice.Value.ToString(CultureInfo.InvariantCulture) : "not set")}\n" +
            $"Similar products in category: {marketData.SimilarProductsCount}\n" +
            $"Observed min price: {marketData.MinObservedPrice}\n" +
            $"Observed max price: {marketData.MaxObservedPrice}\n" +
            $"Observed average price: {marketData.AverageObservedPrice}\n\n" +
            "Suggest a competitive price range and a single recommended price.";

        var history = new List<ChatTurn> { new("user", userPrompt) };
        var raw = await chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        raw = raw.Trim();
        if (raw.StartsWith("```"))
            raw = raw.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var parsed = JsonSerializer.Deserialize<AiPriceJson>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed is not null)
                return (parsed.MinPrice, parsed.MaxPrice, parsed.SuggestedPrice, parsed.Reasoning);
        }
        catch (JsonException)
        {
            // fall through to market-data-only fallback below
        }

        // AI response wasn't parseable - fall back to pure market-data math so the endpoint still returns something useful.
        var avg = marketData.AverageObservedPrice ?? currentPrice ?? 0m;
        return (
            marketData.MinObservedPrice ?? avg,
            marketData.MaxObservedPrice ?? avg,
            avg,
            "Suggestion based on observed market prices (AI explanation unavailable).");
    }

    private sealed class AiPriceJson
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal SuggestedPrice { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}
