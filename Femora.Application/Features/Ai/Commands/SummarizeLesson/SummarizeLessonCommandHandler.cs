using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Commands.SummarizeLesson;

public class SummarizeLessonCommandHandler : IRequestHandler<SummarizeLessonCommand, SummarizeLessonResponse>
{
    private const int TopChunks = 20;

    private readonly IAppDbContext _db;
    private readonly IEmbeddingRepository _embeddingRepository;
    private readonly ISearchIndexRepository _searchIndexRepository;
    private readonly IChatCompletionRepository _chatCompletionRepository;

    public SummarizeLessonCommandHandler(
        IAppDbContext db,
        IEmbeddingRepository embeddingRepository,
        ISearchIndexRepository searchIndexRepository,
        IChatCompletionRepository chatCompletionRepository)
    {
        _db = db;
        _embeddingRepository = embeddingRepository;
        _searchIndexRepository = searchIndexRepository;
        _chatCompletionRepository = chatCompletionRepository;
    }

    public async Task<SummarizeLessonResponse> Handle(SummarizeLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson", request.LessonId.ToString());

        // Broad query embedding to pull back the lesson's most representative chunks
        // (same retrieval pattern used for the key-questions PDF generator).
        var queryEmbedding = await _embeddingRepository.GenerateEmbeddingAsync(
            $"Summary of the lesson: {lesson.Title}",
            cancellationToken);

        var chunks = await _searchIndexRepository.SearchAsync(
            queryEmbedding, top: TopChunks, lessonId: request.LessonId, cancellationToken: cancellationToken);

        var contentText = new StringBuilder();
        if (chunks.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(lesson.ArticleContent))
            {
                contentText.AppendLine(lesson.ArticleContent);
            }
            else if (lesson.Type == LessonType.Video)
            {
                // For video lessons, check if there's a resource and its indexing status
                var resource = await _db.LessonResources
                    .FirstOrDefaultAsync(r => r.LessonId == request.LessonId, cancellationToken);

                if (resource == null)
                {
                    // No resource uploaded yet - return pending status instead of throwing
                    return new SummarizeLessonResponse
                    {
                        LessonId = lesson.Id,
                        Status = "pending",
                        StatusMessage = "No video resource has been uploaded for this lesson. Please upload a video file to enable summarization.",
                        Summary = string.Empty
                    };
                }

                if (resource.Status == LessonIndexingStatus.Pending)
                {
                    // Video is queued but not yet processed
                    return new SummarizeLessonResponse
                    {
                        LessonId = lesson.Id,
                        Status = "processing",
                        StatusMessage = "The video is being processed for indexing. Please wait for the transcription to complete and try again.",
                        Summary = string.Empty
                    };
                }

                if (resource.Status == LessonIndexingStatus.Failed)
                {
                    var errorDetail = !string.IsNullOrWhiteSpace(resource.ErrorMessage)
                        ? $" Error: {resource.ErrorMessage}"
                        : string.Empty;
                    throw new ContentNotIndexedException(
                        $"The video transcription failed and could not be indexed.{errorDetail} Please re-upload the video.");
                }

                // Status is LessonIndexingStatus.Indexed but no chunks were found
                // Fall back to using video metadata (title and duration) for summarization
                contentText.AppendLine($"Video Title: {lesson.Title}");
                if (lesson.DurationSeconds.HasValue)
                {
                    var minutes = lesson.DurationSeconds.Value / 60;
                    contentText.AppendLine($"Video Duration: {minutes} minutes");
                }
                contentText.AppendLine("Note: This summary is based on video metadata only, as the video content could not be extracted. The video may have no audio track or the audio may be inaudible. Consider adding article content for a better summary.");
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
                contentText.AppendLine(chunk.Content).AppendLine();
        }

        var length = request.Length.ToLowerInvariant();
        var lengthInstruction = length switch
        {
            "short" => "Write a very brief summary: 2-3 sentences capturing only the core idea.",
            "detailed" => "Write a thorough, detailed summary covering all major points, sub-topics, and any important examples, as organized bullet points or short paragraphs.",
            _ => "Write a medium-length summary: a short paragraph (4-6 sentences) covering the main points."
        };

        var systemPrompt =
            "You are an educational content summarizer for Femora, a handicrafts/DIY learning platform. " +
            "Summarize the lesson content below faithfully - never invent facts that aren't in it. " +
            $"{lengthInstruction} " +
            "Write the summary in Egyptian Arabic dialect. Respond with the summary text only, no titles, no markdown.";

        var userPrompt = $"Lesson title: {lesson.Title}\n\nContent:\n{contentText}";

        var history = new List<ChatTurn> { new("user", userPrompt) };
        var summary = await _chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        return new SummarizeLessonResponse
        {
            LessonId = lesson.Id,
            Summary = summary.Trim(),
            Status = "completed",
            StatusMessage = string.Empty
        };
    }
}
