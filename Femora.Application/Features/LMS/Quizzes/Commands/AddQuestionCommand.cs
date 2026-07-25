using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record AddQuestionCommand(
    Guid QuizId,
    string Text,
    int OrderIndex
) : IRequest<Guid>;

