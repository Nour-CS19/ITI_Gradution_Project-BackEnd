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
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Commands.ChatWithLesson;

public class ChatWithLessonCommandHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository,
    ISearchIndexRepository searchIndexRepository,
    IChatCompletionRepository chatCompletionRepository)
    : IRequestHandler<ChatWithLessonCommand, ChatWithLessonResponse>
{
    private const int TopChunks = 6;

    public async Task<ChatWithLessonResponse> Handle(ChatWithLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson", request.LessonId.ToString());

        // defer getting/creating conversation until after we confirm lesson content is indexed
        AIConversation? conversation = null;

        // 2. RAG retrieval: embed the question, search this lesson's indexed chunks
        var queryEmbedding = await embeddingRepository.GenerateEmbeddingAsync(request.Question, cancellationToken);

        var chunks = await searchIndexRepository.SearchAsync(
            queryEmbedding,
            top: TopChunks,
            lessonId: request.LessonId,
            cancellationToken: cancellationToken);

        var contextText = new StringBuilder();

        if (chunks.Count == 0)
        {
            // Fallback: if inline article content exists for the lesson, use it as context
            if (!string.IsNullOrWhiteSpace(lesson.ArticleContent))
            {
                contextText.AppendLine(lesson.ArticleContent).AppendLine();
            }
            else
            {
                throw new ContentNotIndexedException(
                    "No indexed content was found for this lesson. Make sure a lesson resource has been uploaded and indexed first.");
            }
        }
        else
        {
            foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
            {
                contextText.AppendLine(chunk.Content).AppendLine();
            }
        }

        // 1b. Now get or create the conversation (after confirming indexed content exists)
        conversation = request.ConversationId.HasValue
            ? await db.AIConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value && c.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("AIConversation", request.ConversationId.Value.ToString())
            : null;

        if (conversation is null)
        {
            conversation = new AIConversation
            {
                UserId = request.UserId,
                Title = lesson.Title is not null && lesson.Title.Length > 0
                    ? $"Chat: {lesson.Title}"
                    : "Lesson Chat",
                Context = AIConversationContext.CourseSupport,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.AIConversations.Add(conversation);
        }

        // 3. Save the user's message
        var userMessage = new AIMessage
        {
            ConversationId = conversation.Id,
            Conversation = conversation,
            Role = AIMessageRole.User,
            Content = request.Question,
            SentAt = DateTime.UtcNow
        };
        db.AIMessages.Add(userMessage);

        // 4. Build chat history (previous turns + new question) for the model
        var history = (conversation.Messages ?? Enumerable.Empty<AIMessage>())
            .OrderBy(m => m.SentAt)
            .Select(m => new ChatTurn(m.Role == AIMessageRole.User ? "user" : "assistant", m.Content))
            .ToList();
        history.Add(new ChatTurn("user", request.Question));

        var systemPrompt =
            "You are a helpful teaching assistant embedded inside a lesson page. Answer the student's " +
            "question using ONLY the lesson context provided below - never invent facts that aren't in it. " +
            "Write a complete, well-structured answer (a short explanation, and a concrete example or a " +
            "bullet list when it helps understanding) rather than a one-line reply, but stay focused on the " +
            "question - do not pad with unrelated content. Answer in the same language the student asked in " +
            "(Arabic or English). If the answer truly isn't in the context, say so honestly instead of guessing.\n\n" +
            $"Lesson context:\n{contextText}";

        // 5. Get the AI's answer
        var answer = await chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        // 6. Save the assistant's reply
        var assistantMessage = new AIMessage
        {
            ConversationId = conversation.Id,
            Conversation = conversation,
            Role = AIMessageRole.Assistant,
            Content = answer,
            SentAt = DateTime.UtcNow
        };
        db.AIMessages.Add(assistantMessage);

        conversation.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new ChatWithLessonResponse
        {
            ConversationId = conversation.Id,
            Answer = answer
        };
    }
}
