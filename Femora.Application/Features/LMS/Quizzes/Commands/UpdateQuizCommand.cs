using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record UpdateQuizCommand(
    Guid QuizId,
    string Title,
    int MinimumPassingScore,
    int MaxAttempts
) : IRequest;