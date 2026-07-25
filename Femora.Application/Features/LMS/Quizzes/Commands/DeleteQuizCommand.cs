using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record DeleteQuizCommand(
    Guid QuizId,
    Guid UserId
) : IRequest;