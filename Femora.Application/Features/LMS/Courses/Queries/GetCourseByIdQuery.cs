using MediatR;
using Femora.Application.Features.LMS.Courses.DTOs;

namespace Femora.Application.Features.LMS.Courses.Queries;

public record GetCourseByIdQuery(
    Guid Id,
    Guid? RequestingUserId = null,
    bool RequestingUserIsAdmin = false
) : IRequest<CourseDetailsDto>;