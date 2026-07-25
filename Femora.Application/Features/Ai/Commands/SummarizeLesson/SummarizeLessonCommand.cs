using MediatR;
using System;

namespace Femora.Application.Features.Ai.Commands.SummarizeLesson;

public record SummarizeLessonCommand : IRequest<SummarizeLessonResponse>
{
    public Guid LessonId { get; init; }

    /// <summary>
    /// "short", "medium", or "detailed" - controls the summary length/depth.
    /// </summary>
    public string Length { get; init; } = "medium";
}

public record SummarizeLessonResponse
{
    public Guid LessonId { get; init; }
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Indicates the status of the summarization request.
    /// "completed" = summary is ready in the Summary property
    /// "pending" = video is being processed, try again later
    /// "processing" = video is currently being indexed
    /// </summary>
    public string Status { get; init; } = "completed";

    /// <summary>
    /// User-friendly message about the current state.
    /// </summary>
    public string StatusMessage { get; init; } = string.Empty;
}
