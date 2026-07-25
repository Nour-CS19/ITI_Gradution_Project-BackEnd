using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Entities.AI;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.AI.Recommendations.Commands.SuggestProductImprovements;

public class SuggestProductImprovementsCommandHandler(
    IAppDbContext db,
    IChatCompletionRepository chatCompletionRepository)
    : IRequestHandler<SuggestProductImprovementsCommand, SuggestProductImprovementsResponse>
{
    public async Task<SuggestProductImprovementsResponse> Handle(SuggestProductImprovementsCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductCategory)
            .Include(p => p.SellerProfile)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId.ToString());

        var listingSummary = BuildListingSummary(product);

        var systemPrompt =
            "You are an e-commerce listing quality expert. Review the product listing data below " +
            "and respond ONLY with a valid JSON object, no markdown. " +
            "Schema: { \"suggestions\": [string, ...], \"overallAssessment\": string }. " +
            "Give 3-6 concrete, actionable suggestions to improve this listing's quality " +
            "(e.g. description completeness, image coverage, title clarity, missing variant details). " +
            "Only flag things that are actually missing or weak based on the data given - do not invent issues. " +
            "overallAssessment should be 1-2 sentences summarizing the listing's current quality.";

        var history = new List<ChatTurn> { new("user", listingSummary) };
        var raw = await chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        raw = raw.Trim();
        if (raw.StartsWith("```"))
            raw = raw.Replace("```json", "").Replace("```", "").Trim();

        List<string> suggestions;
        string assessment;

        try
        {
            var parsed = JsonSerializer.Deserialize<AiImprovementJson>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            suggestions = parsed?.Suggestions ?? new List<string>();
            assessment = parsed?.OverallAssessment ?? string.Empty;
        }
        catch (JsonException)
        {
            suggestions = new List<string> { "AI suggestions could not be parsed. Please try again." };
            assessment = string.Empty;
        }

        db.AIRecommendations.Add(new AIRecommendation
        {
            UserId = product.SellerProfile?.UserId ?? Guid.Empty,
            Type = AIRecommendationType.Product,
            EntityId = product.Id,
            EntityType = "ProductQualityImprovement",
            Score = 1.0,
            ReasonJson = JsonSerializer.Serialize(new { suggestions, assessment }),
            GeneratedAt = DateTime.UtcNow
        });

        if (product.SellerProfile is not null)
            await db.SaveChangesAsync(cancellationToken);

        return new SuggestProductImprovementsResponse
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Suggestions = suggestions,
            OverallAssessment = assessment
        };
    }

    private static string BuildListingSummary(Domain.Entities.Marketplace.Product product)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Product name: {product.Name}");
        sb.AppendLine($"Category: {product.ProductCategory?.Name ?? "Unknown"}");
        sb.AppendLine($"Description: {(string.IsNullOrWhiteSpace(product.Description) ? "(empty)" : product.Description)}");
        sb.AppendLine($"Description length: {product.Description?.Length ?? 0} characters");
        sb.AppendLine($"Number of images: {product.ProductImages.Count}");
        sb.AppendLine($"Number of variants: {product.ProductVariants.Count}");

        foreach (var variant in product.ProductVariants)
            sb.AppendLine($"  - Variant '{variant.Name}': price={variant.Price}, stock={variant.StockQuantity}");

        sb.AppendLine($"Published: {product.IsPuplished}");

        return sb.ToString();
    }

    private sealed class AiImprovementJson
    {
        public List<string> Suggestions { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
    }
}
