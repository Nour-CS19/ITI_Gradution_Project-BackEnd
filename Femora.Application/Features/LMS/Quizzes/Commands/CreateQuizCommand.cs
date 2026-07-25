using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record CreateQuizCommand(
    Guid CourseId,
    Guid? ModuleId,
    string Title,
    int MinimumPassingScore,
    int MaxAttempts
) : IRequest<Guid>;