using System;
using System.Collections.Generic;

namespace Femora.Application.Common.DTOs;

// ============================================================
// RAG pipeline DTOs (lesson indexing)
// ============================================================

public class TextChunk
{
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
}

public class LessonChunkDocument
{
    public string Id { get; set; } = string.Empty;
    public Guid LessonResourceId { get; set; }
    public Guid LessonId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
}

public class LessonChunkSearchResult
{
    public string Id { get; set; } = string.Empty;
    public Guid LessonResourceId { get; set; }
    public Guid LessonId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}

// ============================================================
// AI Quiz Generation DTOs
// ============================================================

public class AIGeneratedQuiz
{
    public List<AIGeneratedQuestion> Questions { get; set; } = new();
}

public class AIGeneratedQuestion
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "MultipleChoice"; // "MultipleChoice" | "TrueFalse"
    // Short exact excerpt copied from the RAG context that this question is grounded on.
    // Empty when the question falls back to the model's own domain knowledge (no indexed
    // content available). Used to reject ungrounded/hallucinated questions - see
    // GenerateQuizCommandHandler.IsGrounded.
    public string SourceQuote { get; set; } = string.Empty;
    public List<AIGeneratedChoice> Choices { get; set; } = new();
}

public class AIGeneratedChoice
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

// ============================================================
// Price Suggestion DTOs
// ============================================================

public class AISuggestedPrice
{
    public decimal SuggestedPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Reasoning { get; set; } = string.Empty;
}

// ============================================================
// AI Controller request DTOs (chatbot / chat-with-lesson / summarize)
// ============================================================

public record SendMessageRequest
{
    public Guid? ConversationId { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ChatWithLessonRequest
{
    public Guid? ConversationId { get; init; }
    public string Question { get; init; } = string.Empty;
}

public record SetInterestsRequest
{
    public List<Guid> InterestIds { get; init; } = new();
}

public record RenameConversationRequest
{
    public string Title { get; init; } = string.Empty;
}

/// <summary>
/// A single tappable "quick question" chip shown above the chat input, both for the
/// general assistant and for the per-lesson Q&amp;A panel.
/// </summary>
public record SuggestedQuestionDto
{
    public string Question { get; init; } = string.Empty;
}
