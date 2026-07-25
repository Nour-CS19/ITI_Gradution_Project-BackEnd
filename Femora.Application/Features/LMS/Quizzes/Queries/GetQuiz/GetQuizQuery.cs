using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Quizzes.Queries.GetQuiz;

public record GetQuizQuery : IRequest<GetQuizResponse>
{
    public Guid QuizId { get; init; }
}

public record GetQuizResponse
{
    public Guid QuizId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid CourseId { get; init; }
    public Guid? ModuleId { get; init; }
    public int MinimumPassingScore { get; init; }
    public int MaxAttempts { get; init; }
    public List<QuizQuestionDto> Questions { get; init; } = new();
}

public record QuizQuestionDto
{
    public Guid QuestionId { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Type { get; init; } = "MultipleChoice";
    public int OrderIndex { get; init; }
    public List<QuizChoiceDto> Choices { get; init; } = new();
}

public record QuizChoiceDto
{
    public Guid ChoiceId { get; init; }
    public string Text { get; init; } = string.Empty;
    public int Order { get; init; }
    // Intentionally NOT including IsCorrect: this DTO is what a trainee
    // sees while taking the quiz, so the correct answer must stay hidden.
}