using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record AddChoiceCommand(
    Guid QuestionId,
    string Text,
    int Order,
    bool IsCorrect
) : IRequest<Guid>;

