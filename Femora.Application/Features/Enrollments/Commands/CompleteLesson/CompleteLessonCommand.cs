using MediatR;

namespace Femora.Application.Features.Enrollments.Commands.CompleteLesson;

public record CompleteLessonCommand(Guid LessonId) : IRequest<CompleteLessonResponse>;
