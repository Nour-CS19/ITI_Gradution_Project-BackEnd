using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Quizzes.Commands.GenerateQuiz;

public class GenerateQuizCommandHandler(
    IAppDbContext db,
    IEmbeddingRepository embeddingRepository,
    ISearchIndexRepository searchIndexRepository,
    IAIQuizGeneratorRepository aiQuizGeneratorRepository)
    : IRequestHandler<GenerateQuizCommand, GenerateQuizResponse>
{
    // How many chunks to pull per lesson when building the RAG context.
    private const int TopChunksPerLesson = 5;

    public async Task<GenerateQuizResponse> Handle(GenerateQuizCommand request, CancellationToken cancellationToken)
    {
        var module = await db.Modules
            .Include(m => m.Lessons)
            .Include(m => m.Quiz)
                .ThenInclude(q => q!.Questions)
                    .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(m => m.Id == request.ModuleId, cancellationToken)
            ?? throw new NotFoundException("Module", request.ModuleId.ToString());

        // Idempotency: a module should only ever have ONE quiz. Without this check,
        // every retry/reload from the frontend (which always calls "generate") created a
        // brand-new Quiz row for the same module, leaving orphaned quizzes with no
        // attempts and making "has this trainee passed the module quiz" ambiguous.
        if (module.Quiz is not null)
        {
            return MapToResponse(module.Quiz);
        }

        if (module.Lessons.Count == 0)
            throw new InvalidOperationException("This module has no lessons to generate a quiz from.");

        string contextText;
        try
        {
            contextText = await BuildModuleContextAsync(module.Lessons.Select(l => l.Id), cancellationToken);
        }
        catch (Exception)
        {
            // Azure AI Search / embedding call itself failed (network, auth, index not
            // provisioned yet, etc). Fall through to the lesson-text fallback below instead
            // of blowing up the whole request.
            contextText = string.Empty;
        }

        // Fall back to the lessons' own text (title + article content) when nothing has
        // been indexed into Azure AI Search yet. Previously this threw and completing the
        // last lesson of a module would fail outright with no quiz ever created.
        if (string.IsNullOrWhiteSpace(contextText))
        {
            contextText = BuildFallbackContextFromLessons(module.Lessons);
        }

        if (string.IsNullOrWhiteSpace(contextText))
            throw new InvalidOperationException(
                "This module's lessons have no indexed content and no text content (title/article) to generate a quiz from.");

        var aiResult = await aiQuizGeneratorRepository.GenerateQuizAsync(
            topicTitle: module.Title,
            contextText: contextText,
            questionCount: request.QuestionCount,
            choicesPerQuestion: request.ChoicesPerQuestion,
            cancellationToken: cancellationToken);

        var quiz = new Quiz
        {
            CourseId = module.CourseId,
            ModuleId = module.Id,
            Title = $"{module.Title} - Quiz",
            MinimumPassingScore = request.MinimumPassingScore,
            MaxAttempts = request.MaxAttempts,
            CreatedAt = DateTime.UtcNow
        };

        var orderIndex = 0;
        var droppedUngrounded = 0;
        foreach (var aiQuestion in aiResult.Questions)
        {
            if (string.IsNullOrWhiteSpace(aiQuestion.Text) || aiQuestion.Choices.Count == 0)
                continue;

            // Anti-hallucination check: reject any question the model claims is grounded
            // in the context but whose "sourceQuote" cannot actually be found in that
            // context. An empty sourceQuote is allowed (explicit fallback-to-domain-
            // knowledge case), but a fabricated quote is a strong hallucination signal.
            if (!IsGrounded(aiQuestion.SourceQuote, contextText))
            {
                droppedUngrounded++;
                continue;
            }

            var questionType = string.Equals(aiQuestion.Type, "TrueFalse", StringComparison.OrdinalIgnoreCase)
                ? QuestionType.TrueFalse
                : QuestionType.MultipleChoice;

            var question = new Question
            {
                Text = aiQuestion.Text,
                Type = questionType,
                OrderIndex = orderIndex
            };

            var choiceOrder = 0;
            foreach (var aiChoice in aiQuestion.Choices)
            {
                question.Choices.Add(new Choice
                {
                    Text = aiChoice.Text,
                    Order = choiceOrder++,
                    IsCorrect = aiChoice.IsCorrect
                });
            }

            // Defensive checks: AI must produce exactly one correct choice per question,
            // and a TrueFalse question must have exactly 2 choices.
            if (question.Choices.Count(c => c.IsCorrect) != 1)
                continue;

            if (questionType == QuestionType.TrueFalse && question.Choices.Count != 2)
                continue;

            orderIndex++;
            quiz.Questions.Add(question);
        }

        if (quiz.Questions.Count == 0)
            throw new InvalidOperationException(
                droppedUngrounded > 0
                    ? "AI generation did not produce any valid, context-grounded questions."
                    : "AI generation did not produce any valid questions.");

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync(cancellationToken);

        return new GenerateQuizResponse
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            Questions = quiz.Questions.Select(q => new GeneratedQuestionDto
            {
                QuestionId = q.Id,
                Text = q.Text,
                Type = q.Type.ToString(),
                Choices = q.Choices.Select(c => new GeneratedChoiceDto
                {
                    ChoiceId = c.Id,
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            }).ToList()
        };
    }

    private static GenerateQuizResponse MapToResponse(Quiz quiz) => new()
    {
        QuizId = quiz.Id,
        Title = quiz.Title,
        Questions = quiz.Questions.Select(q => new GeneratedQuestionDto
        {
            QuestionId = q.Id,
            Text = q.Text,
            Type = q.Type.ToString(),
            Choices = q.Choices.Select(c => new GeneratedChoiceDto
            {
                ChoiceId = c.Id,
                Text = c.Text,
                IsCorrect = c.IsCorrect
            }).ToList()
        }).ToList()
    };

    /// <summary>
    /// Grounding/citation check: verifies the AI's claimed "sourceQuote" is an actual
    /// (near-)verbatim substring of the RAG context that was fed to it. This is the
    /// concrete guard against hallucinated questions - if the model can't quote the
    /// material it says it used, the question is dropped instead of trusted blindly.
    /// An empty quote is treated as an explicit, allowed fallback-to-domain-knowledge case.
    /// </summary>
    private static bool IsGrounded(string? sourceQuote, string contextText)
    {
        if (string.IsNullOrWhiteSpace(sourceQuote))
            return true;

        if (string.IsNullOrWhiteSpace(contextText))
            return false;

        return Normalize(contextText).Contains(Normalize(sourceQuote));
    }

    private static string Normalize(string text) =>
        Regex.Replace(text.ToLowerInvariant(), @"\s+", " ").Trim();

    /// <summary>
    /// Builds context straight from each lesson's own title + article text, used only
    /// when Azure AI Search has no indexed chunks yet (e.g. indexing pipeline hasn't run,
    /// or the lesson has no uploaded resource). Keeps quiz generation working end-to-end
    /// even before the RAG pipeline is fully wired up.
    /// </summary>
    private static string BuildFallbackContextFromLessons(IEnumerable<Domain.Entities.LMS.Lesson> lessons)
    {
        var builder = new StringBuilder();
        foreach (var lesson in lessons)
        {
            if (string.IsNullOrWhiteSpace(lesson.Title) && string.IsNullOrWhiteSpace(lesson.ArticleContent))
                continue;

            builder.AppendLine($"=== Lesson: {lesson.Title} ===");
            if (!string.IsNullOrWhiteSpace(lesson.ArticleContent))
                builder.AppendLine(lesson.ArticleContent);
            builder.AppendLine();
        }
        return builder.ToString().Trim();
    }

    /// <summary>
    /// Builds a RAG context by pulling the top indexed chunks for each lesson
    /// in the module from Azure AI Search, using the module title as the
    /// semantic query (embedded), then concatenating them into one text blob.
    /// </summary>
    private async Task<string> BuildModuleContextAsync(IEnumerable<Guid> lessonIds, CancellationToken cancellationToken)
    {
      
        var queryEmbedding = await embeddingRepository.GenerateEmbeddingAsync(
            "Key concepts, definitions and important facts covered in this lesson",
            cancellationToken);

        var contextBuilder = new StringBuilder();

        /*  foreach (var lessonId in lessonIds)
          {
              var chunks = await searchIndexRepository.SearchAsync(
                  queryEmbedding,
                  top: TopChunksPerLesson,
                  lessonId: lessonId,
                  cancellationToken: cancellationToken);

              foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
                  contextBuilder.AppendLine(chunk.Content).AppendLine();
          }*/
        foreach (var lessonId in lessonIds)
        {
            var chunks = await searchIndexRepository.SearchAsync(
                queryEmbedding,
                top: TopChunksPerLesson,
                lessonId: lessonId,
                cancellationToken: cancellationToken);

            if (!chunks.Any()) continue; // ✅ تجاهل الـ lessons الفاضية

            contextBuilder.AppendLine($"=== Lesson: {lessonId} ==="); // ✅ فاصل واضح

            foreach (var chunk in chunks.OrderBy(c => c.ChunkIndex))
                contextBuilder.AppendLine(chunk.Content).AppendLine();
        }

        return contextBuilder.ToString().Trim();
    }
}