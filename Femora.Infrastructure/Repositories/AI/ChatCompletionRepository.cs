using Azure;
using Azure.AI.OpenAI;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class ChatCompletionRepository : IChatCompletionRepository
{
    private readonly ChatClient _chatClient;

    public ChatCompletionRepository(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;
        var azureClient = new AzureOpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        _chatClient = azureClient.GetChatClient(settings.ChatDeploymentName);
    }

    public async Task<string> CompleteChatAsync(string systemPrompt, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

        foreach (var turn in history)
        {
            ChatMessage message = turn.Role.ToLowerInvariant() switch
            {
                "user" => new UserChatMessage(turn.Content),
                "assistant" => new AssistantChatMessage(turn.Content),
                _ => new SystemChatMessage(turn.Content)
            };
            messages.Add(message);
        }

        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0.5f,
            MaxOutputTokenCount = 2048
        };

        var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
        return response.Value.Content[0].Text?.Trim() ?? string.Empty;
    }
}
