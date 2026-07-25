using MediatR;
using System;

namespace Femora.Application.Features.Ai.Commands.SendMessage;

public record SendMessageCommand : IRequest<SendMessageResponse>
{
    public Guid UserId { get; init; }

    /// <summary>
    /// Existing conversation to continue. If null, a new conversation is created.
    /// </summary>
    public Guid? ConversationId { get; init; }

    public string Message { get; init; } = string.Empty;
}

public record SendMessageResponse
{
    public Guid ConversationId { get; init; }
    public string Reply { get; init; } = string.Empty;
}
