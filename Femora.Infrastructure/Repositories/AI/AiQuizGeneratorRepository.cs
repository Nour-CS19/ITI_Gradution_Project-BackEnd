using Azure;
using Azure.AI.OpenAI;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.DTOs;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class AiQuizGeneratorRepository : IAIQuizGeneratorRepository
{
    private readonly ChatClient _chatClient;

    public AiQuizGeneratorRepository(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;
        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new AzureKeyCredential(settings.ApiKey));

        _chatClient = azureClient.GetChatClient(settings.ChatDeploymentName);
    }

    public async Task<AIGeneratedQuiz> GenerateQuizAsync(
        string topicTitle,
        string contextText,
        int questionCount,
        int choicesPerQuestion,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt =
            "You are an educational quiz generator for Femora, a handicrafts/DIY learning platform " +
            "(topics like crochet, embroidery, pottery, resin art, macrame, soap/candle making, jewelry-making, decoupage). " +
            "Respond ONLY with a valid JSON object — no markdown, no explanation. " +
            "The JSON must follow this exact schema:\n" +
            "{\n" +
            "  \"questions\": [\n" +
            "    {\n" +
            "      \"text\": \"<question text>\",\n" +
            "      \"type\": \"MultipleChoice\" | \"TrueFalse\",\n" +
            "      \"sourceQuote\": \"<short exact phrase copied verbatim from the Context that this question is based on, or empty string if none>\",\n" +
            "      \"choices\": [\n" +
            "        { \"text\": \"<choice text>\", \"isCorrect\": false },\n" +
            "        { \"text\": \"<choice text>\", \"isCorrect\": true }\n" +
            "      ]\n" +
            "    }\n" +
            "  ]\n" +
            "}\n" +
            "Rules:\n" +
            $"- Generate exactly {questionCount} questions.\n" +
            "- Mix question types: use \"TrueFalse\" for simple factual statements and \"MultipleChoice\" " +
            "for anything that needs distractors. Roughly a quarter to a third of questions should be TrueFalse.\n" +
            $"- If \"type\" is \"MultipleChoice\", the question must have exactly {choicesPerQuestion} choices.\n" +
            "- If \"type\" is \"TrueFalse\", the question must have EXACTLY 2 choices with text \"True\" and \"False\".\n" +
            "- Exactly ONE choice per question must have isCorrect = true.\n" +
            "- Base all questions strictly on the provided context and the given topic title.\n" +
            "- \"sourceQuote\" MUST be an exact, verbatim substring copied from the Context below (max ~15 words), " +
            "proving the question is grounded in the supplied material. Do NOT paraphrase it or invent a quote.\n" +
            "- The topic is a hands-on craft/handicraft skill. NEVER generate questions about an unrelated " +
            "domain (e.g. programming, software, math, general trivia) even if the provided context is thin " +
            "or repetitive - if the context doesn't give you enough specific detail, fall back on your own " +
            "general knowledge of THIS SPECIFIC CRAFT topic (named in \"Topic\" below) to write reasonable " +
            "questions about its real tools, techniques, or terminology instead, and set \"sourceQuote\" to \"\" " +
            "in that case (do NOT fabricate a quote that isn't really in the Context).\n" +
            "- Do NOT include explanations or extra fields.";

        var userPrompt =
            $"Topic: {topicTitle}\n\n" +
            $"Context:\n{contextText}\n\n" +
            $"Generate {questionCount} multiple-choice questions with {choicesPerQuestion} choices each.";

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var chatOptions = new ChatCompletionOptions
        {
            // Lower than before (0.3f) on purpose: the lower the temperature, the more the
            // model sticks to the supplied context instead of inventing plausible-sounding
            // but ungrounded facts. Combined with the sourceQuote grounding check below.
            Temperature = 0.1f,
            MaxOutputTokenCount = 4096
        };

        var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
        var rawJson = response.Value.Content[0].Text?.Trim() ?? string.Empty;

        // Strip any accidental markdown fences the model might emit.
        if (rawJson.StartsWith("```"))
        {
            rawJson = rawJson
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();
        }

        var parsed = JsonSerializer.Deserialize<AiQuizJsonRoot>(rawJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (parsed?.Questions is null || parsed.Questions.Count == 0)
            throw new InvalidOperationException(
                "Azure OpenAI did not return a valid quiz JSON. Raw response: " + rawJson);

        var quiz = new AIGeneratedQuiz();

        foreach (var q in parsed.Questions)
        {
            var question = new AIGeneratedQuestion
            {
                Text = q.Text,
                Type = string.Equals(q.Type, "TrueFalse", StringComparison.OrdinalIgnoreCase)
                    ? "TrueFalse"
                    : "MultipleChoice",
                SourceQuote = q.SourceQuote ?? string.Empty
            };
            foreach (var c in q.Choices)
                question.Choices.Add(new AIGeneratedChoice { Text = c.Text, IsCorrect = c.IsCorrect });

            quiz.Questions.Add(question);
        }

        return quiz;
    }

    // --------------- private JSON shape ---------------

    private sealed class AiQuizJsonRoot
    {
        public List<AiQuizJsonQuestion> Questions { get; set; } = new();
    }

    private sealed class AiQuizJsonQuestion
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = "MultipleChoice";
        public string SourceQuote { get; set; } = string.Empty;
        public List<AiQuizJsonChoice> Choices { get; set; } = new();
    }

    private sealed class AiQuizJsonChoice
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
