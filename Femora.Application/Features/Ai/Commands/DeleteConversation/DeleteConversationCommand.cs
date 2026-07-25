using MediatR;
using System;

namespace Femora.Application.Features.Ai.Commands.DeleteConversation;

public record DeleteConversationCommand : IRequest<Unit>
{
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
}
