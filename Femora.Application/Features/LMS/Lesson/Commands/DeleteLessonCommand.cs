using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public record DeleteLessonCommand(Guid LessonId) : IRequest;