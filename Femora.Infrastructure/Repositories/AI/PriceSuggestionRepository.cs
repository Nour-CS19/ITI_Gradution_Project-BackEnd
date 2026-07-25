using Azure;
using Azure.AI.OpenAI;
using Femora.Application.Common.DTOs;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class PriceSuggestionRepository : IPriceSuggestionRepository
{
    private readonly ChatClient _chatClient;

    public PriceSuggestionRepository(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;
        var azureClient = new AzureOpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        _chatClient = azureClient.GetChatClient(settings.ChatDeploymentName);
    }

    public async Task<AISuggestedPrice> SuggestPriceAsync(
        string productName,
        string? description,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt =
            "You are a pricing assistant specialized in the Egyptian e-commerce market (prices in EGP). " +
            "Given a product's name, description, and category, suggest a fair, competitive market price " +
            "based on typical pricing for similar products sold in Egypt (e.g. on platforms like Jumia Egypt, " +
            "Noon Egypt, or local marketplaces). " +
            "Respond ONLY with a valid JSON object - no markdown, no explanation outside the JSON. " +
            "The JSON must follow this exact schema:\n" +
            "{\n" +
            "  \"suggestedPrice\": <number>,\n" +
            "  \"minPrice\": <number>,\n" +
            "  \"maxPrice\": <number>,\n" +
            "  \"reasoning\": \"<short 1-2 sentence explanation in Arabic or English>\"\n" +
            "}\n" +
            "Rules:\n" +
            "- All prices are in Egyptian Pounds (EGP), integers or up to 2 decimal places.\n" +
            "- minPrice and maxPrice represent a realistic competitive range.\n" +
            "- suggestedPrice should sit within [minPrice, maxPrice].\n" +
            "- Base your estimate on realistic current Egyptian market conditions.";

        var userPrompt =
            $"Product name: {productName}\n" +
            $"Category: {categoryName}\n" +
            $"Description: {description ?? "(no description provided)"}\n\n" +
            "Suggest a fair market price in EGP for this product in the Egyptian market.";

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0.3f,
            MaxOutputTokenCount = 512
        };

        var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
        var rawJson = response.Value.Content[0].Text?.Trim() ?? string.Empty;

        if (rawJson.StartsWith("```"))
        {
            rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
        }

        var parsed = JsonSerializer.Deserialize<PriceSuggestionJson>(rawJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed is null)
        {
            throw new InvalidOperationException("Azure OpenAI did not return a valid price suggestion JSON. Raw response: " + rawJson);
        }

        return new AISuggestedPrice
        {
            SuggestedPrice = parsed.SuggestedPrice,
            MinPrice = parsed.MinPrice,
            MaxPrice = parsed.MaxPrice,
            Currency = "EGP",
            Reasoning = parsed.Reasoning
        };
    }

    private sealed class PriceSuggestionJson
    {
        public decimal SuggestedPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }
}
