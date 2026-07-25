using MediatR;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record DeleteCourseCommand(
    Guid CourseId,
    Guid UserId
) : IRequest;