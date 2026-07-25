using MediatR;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record PublishCourseCommand(
    Guid CourseId,
    Guid UserId
) : IRequest;