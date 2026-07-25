using MediatR;
using System;

namespace Femora.Application.Features.Ai.Commands.RenameConversation;

public record RenameConversationCommand : IRequest<Unit>
{
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
}
