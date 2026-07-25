using MediatR;
using System;

namespace Femora.Application.Features.Ai.Commands.ChatWithLesson;

public record ChatWithLessonCommand : IRequest<ChatWithLessonResponse>
{
    public Guid UserId { get; init; }
    public Guid LessonId { get; init; }

    /// <summary>
    /// Existing conversation to continue. If null, a new conversation is created.
    /// </summary>
    public Guid? ConversationId { get; init; }

    public string Question { get; init; } = string.Empty;
}

public record ChatWithLessonResponse
{
    public Guid ConversationId { get; init; }
    public string Answer { get; init; } = string.Empty;
}
