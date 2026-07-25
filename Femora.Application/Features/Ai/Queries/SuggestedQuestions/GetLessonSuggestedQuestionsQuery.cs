using Femora.Application.Common.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Ai.Queries.SuggestedQuestions;

/// <summary>
/// Returns a small rotating set of "quick question" chips for the per-lesson Q&amp;A
/// panel, templated around the lesson's own title - tapping one sends it straight to
/// /api/ai/lessons/{lessonId}/chat, and the free-text box stays available for anything else.
/// </summary>
public record GetLessonSuggestedQuestionsQuery : IRequest<List<SuggestedQuestionDto>>
{
    public Guid LessonId { get; init; }
    public int Count { get; init; } = 4;
}
