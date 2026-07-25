using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Ai.Queries.GetConversation;

public record GetConversationQuery : IRequest<GetConversationResponse>
{
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
}

public record GetConversationResponse
{
    public Guid ConversationId { get; init; }
    public string? Title { get; init; }
    public List<MessageDto> Messages { get; init; } = new();
}

public record MessageDto
{
    public Guid MessageId { get; init; }
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
}
