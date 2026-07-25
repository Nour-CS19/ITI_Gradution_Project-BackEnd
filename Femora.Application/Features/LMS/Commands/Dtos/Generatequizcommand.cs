using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Quizzes.Commands.GenerateQuiz;


public record GenerateQuizCommand : IRequest<GenerateQuizResponse>
{
    public Guid ModuleId { get; init; }

    public int QuestionCount { get; init; } = 10;
    public int ChoicesPerQuestion { get; init; } = 4;

    public int MinimumPassingScore { get; init; } = 60;
    public int MaxAttempts { get; init; } = 3;
}

public record GenerateQuizResponse
{
    public Guid QuizId { get; init; }
    public string Title { get; init; } = string.Empty;
    public List<GeneratedQuestionDto> Questions { get; init; } = new();
}

public record GeneratedQuestionDto
{
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Type { get; init; } = "MultipleChoice";
    public List<GeneratedChoiceDto> Choices { get; init; } = new();
}

public record GeneratedChoiceDto
{
    public Guid ChoiceId { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}