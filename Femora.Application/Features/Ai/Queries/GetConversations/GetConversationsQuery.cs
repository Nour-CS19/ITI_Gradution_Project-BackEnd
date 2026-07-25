using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Ai.Queries.GetConversations;

public record GetConversationsQuery : IRequest<List<ConversationSummaryDto>>
{
    public Guid UserId { get; init; }
}

public record ConversationSummaryDto
{
    public Guid ConversationId { get; init; }
    public string? Title { get; init; }
    public string Context { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
