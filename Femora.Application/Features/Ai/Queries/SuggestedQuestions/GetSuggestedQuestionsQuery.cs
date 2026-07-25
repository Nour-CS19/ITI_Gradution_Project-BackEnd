using Femora.Application.Common.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Ai.Queries.SuggestedQuestions;

/// <summary>
/// Returns a small rotating set of "quick question" chips for the general chatbot's
/// empty state - tapping one sends it straight to /api/ai/chat, and the free-text
/// input stays available for anything else.
/// </summary>
public record GetSuggestedQuestionsQuery : IRequest<List<SuggestedQuestionDto>>
{
    public Guid UserId { get; init; }
    public int Count { get; init; } = 5;
}
