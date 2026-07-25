using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface IChatCompletionRepository
{
    /// <summary>
    /// Sends a system prompt + conversation history to the chat model and returns the assistant's reply.
    /// </summary>
    Task<string> CompleteChatAsync(string systemPrompt, IReadOnlyList<ChatTurn> history, CancellationToken cancellationToken = default);
}

public record ChatTurn(string Role, string Content);
