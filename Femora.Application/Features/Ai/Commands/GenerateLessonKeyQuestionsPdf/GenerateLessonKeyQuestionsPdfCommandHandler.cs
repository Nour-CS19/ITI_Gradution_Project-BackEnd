using System.Text;
using System.Text.Json;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Ai.Commands.GenerateLessonKeyQuestionsPdf;

public class GenerateLessonKeyQuestionsPdfCommandHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository,
    ISearchIndexRepository searchIndexRepository,
    IChatCompletionRepository chatCompletionRepository,
    ILessonPdfRepository pdfRepository,
    IBlobStorageRepository blobStorageRepository)
    : IRequestHandler<GenerateLessonKeyQuestionsPdfCommand, GenerateLessonKeyQuestionsPdfResponse>
{
    private const int TopChunks = 20;

    public async Task<GenerateLessonKeyQuestionsPdfResponse> Handle(
        GenerateLessonKeyQuestionsPdfCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Include(l => l.Module).ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson", request.LessonId.ToString());

        var queryEmbedding = await embeddingRepository.GenerateEmbeddingAsync(
            "Key concepts, definitions, and important facts covered in this lesson",
            cancellationToken);

        var chunks = await searchIndexRepository.SearchAsync(
            queryEmbedding, top: TopChunks, lessonId: request.LessonId, cancellationToken: cancellationToken);

        var contentText = new StringBuilder();
        if (chunks.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(lesson.ArticleContent))
                contentText.AppendLine(lesson.ArticleContent);
            else
                throw new ContentNotIndexedException(
                    "No indexed content was found for this lesson. Make sure a lesson resource has been uploaded and indexed first.");
        }
        else
        {
            foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
                contentText.AppendLine(chunk.Content).AppendLine();
        }

        var questionCount = Math.Clamp(request.QuestionCount, 3, 15);

        var systemPrompt =
            "You are an educational content creator for Femora, a handicrafts/DIY learning platform. " +
            "Given a lesson's content, produce the most important questions a trainee should be able to " +
            "answer to prove they understood it, each with a concise, accurate answer. " +
            "Write everything in Egyptian Arabic dialect. " +
            "Base every question and answer strictly on the provided content - never invent facts. " +
            "Respond ONLY with a valid JSON object, no markdown, matching exactly:\n" +
            "{ \"items\": [ { \"question\": \"<question in Arabic>\", \"answer\": \"<short answer in Arabic>\" } ] }";

        var userPrompt =
            $"Lesson title: {lesson.Title}\n\n" +
            $"Content:\n{contentText}\n\n" +
            $"Generate exactly {questionCount} question/answer pairs covering the most important points.";

        var history = new List<ChatTurn> { new("user", userPrompt) };
        var rawJson = await chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        rawJson = rawJson.Trim();
        if (rawJson.StartsWith("```"))
            rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();

        var items = new List<PdfQuestionItem>();
        try
        {
            var parsed = JsonSerializer.Deserialize<KeyQuestionsJsonRoot>(
                rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.Items != null)
            {
                items = parsed.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Question) && !string.IsNullOrWhiteSpace(i.Answer))
                    .Select(i => new PdfQuestionItem(i.Question!, i.Answer!))
                    .ToList();
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Azure OpenAI did not return a valid key-questions JSON. Raw response: " + rawJson);
        }

        if (items.Count == 0)
            throw new InvalidOperationException("The AI did not return any usable questions for this lesson.");

        var pdfBytes = pdfRepository.GenerateKeyQuestionsPdf(
            lesson.Title, lesson.Module?.Course?.Title ?? string.Empty, items);

        var fileName = $"{Sanitize(lesson.Title)}-key-questions.pdf";
        using var stream = new MemoryStream(pdfBytes);
        var pdfUrl = await blobStorageRepository.UploadFileAsync(
            stream, fileName, "application/pdf", folder: "lesson-key-questions", cancellationToken);

        return new GenerateLessonKeyQuestionsPdfResponse
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            PdfUrl = pdfUrl,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static string Sanitize(string title)
    {
        var invalid = Path.GetInvalidFileNameChars().Concat(new[] { ' ' }).ToArray();
        var cleaned = new string(title.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "lesson" : cleaned.Trim('-');
    }

    private sealed class KeyQuestionsJsonRoot
    {
        public List<KeyQuestionsJsonItem> Items { get; set; } = new();
    }

    private sealed class KeyQuestionsJsonItem
    {
        public string? Question { get; set; }
        public string? Answer { get; set; }
    }
}
