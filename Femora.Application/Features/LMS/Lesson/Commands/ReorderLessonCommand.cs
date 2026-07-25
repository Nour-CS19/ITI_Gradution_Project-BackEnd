using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public record ReorderLessonCommand(
    Guid LessonId,
    int NewOrderIndex
) : IRequest;