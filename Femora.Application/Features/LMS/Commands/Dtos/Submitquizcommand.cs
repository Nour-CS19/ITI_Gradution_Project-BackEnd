using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.LMS.Quizzes.Commands.SubmitQuiz;

public record SubmitQuizCommand : IRequest<SubmitQuizResponse>
{
    public Guid QuizId { get; init; }
    public Guid TraineeProfileId { get; init; }

    public List<SubmittedAnswerDto> Answers { get; init; } = new();
    public object EnrollmentId { get; internal set; }
}

public record SubmittedAnswerDto
{
    public Guid QuestionId { get; init; }
    public Guid ChoiceId { get; init; }
}

public record SubmitQuizResponse
{
    public Guid QuizAttemptId { get; init; }
    public decimal Score { get; init; }
    public int MaxScore { get; init; }
    public bool IsPassed { get; init; }
    public int AttemptNumber { get; init; }
    public List<SubmittedAnswerResultDto> Results { get; init; } = new();
}

public record SubmittedAnswerResultDto
{
    public Guid QuestionId { get; init; }
    public Guid ChoiceId { get; init; }
    public bool IsCorrect { get; init; }
    public Guid CorrectChoiceId { get; init; }
}