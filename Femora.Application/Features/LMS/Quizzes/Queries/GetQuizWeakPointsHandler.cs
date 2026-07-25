using System.Text;
using System.Text.Json;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using Femora.Domain.Entities.LMS.Quizzes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuizWeakPointsHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository,
    ISearchIndexRepository searchIndexRepository,
    IChatCompletionRepository chatCompletionRepository)
    : IRequestHandler<GetQuizWeakPointsQuery, QuizWeakPointsReportDto>
{
    // Chunks pulled per lesson in the module - kept small since we merge across
    // every lesson in the module and only need enough to ground a few explanations.
    private const int ChunksPerLesson = 6;

    public async Task<QuizWeakPointsReportDto> Handle(GetQuizWeakPointsQuery request, CancellationToken cancellationToken)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions).ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quiz), request.QuizId.ToString());

        var traineeProfile = await db.TraineeProfiles
            .FirstOrDefaultAsync(tp => tp.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("TraineeProfile", request.UserId.ToString());

        var attempts = await db.QuizAttempts
            .Where(a => a.QuizId == quiz.Id && a.EnrollmentId == request.EnrollmentId)
            .Include(a => a.Answers)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);

        if (attempts.Count < quiz.MaxAttempts)
            throw new InvalidOperationException(
                "You still have regular attempts left on this quiz - the weak-points review only unlocks after they're used up.");

        var lastAttempt = attempts.First();
        if (lastAttempt.IsPassed)
            throw new InvalidOperationException("This quiz was already passed - no review needed.");

        var existingGrant = await db.QuizRetryGrants
            .FirstOrDefaultAsync(g => g.QuizId == quiz.Id && g.EnrollmentId == request.EnrollmentId, cancellationToken);

        var questionsById = quiz.Questions.ToDictionary(q => q.Id);
        var wrongAnswers = lastAttempt.Answers.Where(a => !a.IsCorrect).ToList();

        if (wrongAnswers.Count == 0)
            throw new InvalidOperationException("No incorrect answers were found on the last attempt to review.");

        // Ground the explanations in the module's actual lesson content, same RAG
        // approach as SummarizeLessonCommandHandler - one broad query per lesson.
        var moduleLessons = quiz.ModuleId.HasValue
            ? await db.Lessons.Where(l => l.ModuleId == quiz.ModuleId.Value).ToListAsync(cancellationToken)
            : new List<Domain.Entities.LMS.Lesson>();

        var contentText = new StringBuilder();
        if (moduleLessons.Count > 0)
        {
            var queryEmbedding = await embeddingRepository.GenerateEmbeddingAsync(
                "Key concepts, definitions, and important facts covered in this lesson",
                cancellationToken);

            foreach (var lesson in moduleLessons)
            {
                var chunks = await searchIndexRepository.SearchAsync(
                    queryEmbedding, top: ChunksPerLesson, lessonId: lesson.Id, cancellationToken: cancellationToken);

                if (chunks.Count == 0 && !string.IsNullOrWhiteSpace(lesson.ArticleContent))
                {
                    contentText.AppendLine(lesson.ArticleContent).AppendLine();
                    continue;
                }

                foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
                    contentText.AppendLine(chunk.Content).AppendLine();
            }
        }

        var wrongItems = wrongAnswers
            .Select(a => questionsById.TryGetValue(a.QuestionId, out var q) ? q : null)
            .Where(q => q != null)
            .Select(q => new
            {
                Question = q!,
                ChosenChoice = q!.Choices.FirstOrDefault(c => c.Id == wrongAnswers.First(a => a.QuestionId == q.Id).ChoiceId),
                CorrectChoice = q!.Choices.FirstOrDefault(c => c.IsCorrect)
            })
            .ToList();

        var systemPrompt =
            "You are a friendly educational tutor for Femora, a handicrafts/DIY learning platform. " +
            "A trainee failed a quiz and you must explain, per question, why their answer was wrong and the " +
            "correct one is right - grounded strictly in the provided lesson content when it's relevant. " +
            "Write in Egyptian Arabic dialect, warm and encouraging, never condescending. " +
            "Respond ONLY with a valid JSON object, no markdown, matching exactly:\n" +
            "{\n" +
            "  \"items\": [ { \"questionId\": \"<guid>\", \"explanation\": \"<2-3 sentence explanation in Egyptian Arabic>\" } ],\n" +
            "  \"overallTip\": \"<one short, encouraging study tip in Egyptian Arabic covering all the weak points>\"\n" +
            "}";

        var userPromptBuilder = new StringBuilder();
        userPromptBuilder.AppendLine($"Quiz: {quiz.Title}");
        userPromptBuilder.AppendLine();
        userPromptBuilder.AppendLine("Lesson content (may be partial):");
        userPromptBuilder.AppendLine(contentText.Length > 0 ? contentText.ToString() : "(no indexed content available)");
        userPromptBuilder.AppendLine();
        userPromptBuilder.AppendLine("Questions the trainee got wrong:");
        foreach (var item in wrongItems)
        {
            userPromptBuilder.AppendLine(
                $"- questionId: {item.Question.Id}; question: \"{item.Question.Text}\"; " +
                $"trainee's answer: \"{item.ChosenChoice?.Text ?? "(no answer)"}\"; " +
                $"correct answer: \"{item.CorrectChoice?.Text ?? "(unknown)"}\"");
        }

        var history = new List<ChatTurn> { new("user", userPromptBuilder.ToString()) };
        var rawJson = await chatCompletionRepository.CompleteChatAsync(systemPrompt, history, cancellationToken);

        rawJson = rawJson.Trim();
        if (rawJson.StartsWith("```"))
            rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();

        string overallTip = "راجعي الدرس تاني بتركيز على النقط اللي غلطتي فيها قبل ما تجربي تاني.";
        var explanationsByQuestion = new Dictionary<Guid, string>();

        try
        {
            var parsed = JsonSerializer.Deserialize<WeakPointsJsonRoot>(
                rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.Items != null)
            {
                foreach (var item in parsed.Items)
                {
                    if (Guid.TryParse(item.QuestionId, out var qid))
                        explanationsByQuestion[qid] = item.Explanation ?? string.Empty;
                }
            }

            if (!string.IsNullOrWhiteSpace(parsed?.OverallTip))
                overallTip = parsed!.OverallTip;
        }
        catch (JsonException)
        {
            // Fall back to the generic tip above and empty per-question explanations
            // rather than failing the whole request over a malformed AI response.
        }

        var report = new QuizWeakPointsReportDto
        {
            QuizId = quiz.Id,
            QuizAttemptId = lastAttempt.Id,
            OverallTip = overallTip,
            WeakPoints = wrongItems.Select(item => new QuizWeakPointItemDto
            {
                QuestionId = item.Question.Id,
                QuestionText = item.Question.Text,
                YourAnswer = item.ChosenChoice?.Text ?? "(لم تتم الإجابة)",
                CorrectAnswer = item.CorrectChoice?.Text ?? "",
                Explanation = explanationsByQuestion.TryGetValue(item.Question.Id, out var exp) && !string.IsNullOrWhiteSpace(exp)
                    ? exp
                    : "راجعي هذه النقطة في محتوى الدرس."
            }).ToList()
        };

        if (existingGrant == null)
        {
            db.QuizRetryGrants.Add(new QuizRetryGrant
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                EnrollmentId = request.EnrollmentId,
                TraineeProfileId = traineeProfile.Id,
                GrantedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            report.RetryUnlocked = true;
        }
        else
        {
            report.RetryUnlocked = !existingGrant.IsUsed;
        }

        return report;
    }

    private sealed class WeakPointsJsonRoot
    {
        public List<WeakPointsJsonItem> Items { get; set; } = new();
        public string? OverallTip { get; set; }
    }

    private sealed class WeakPointsJsonItem
    {
        public string QuestionId { get; set; } = string.Empty;
        public string? Explanation { get; set; }
    }
}
