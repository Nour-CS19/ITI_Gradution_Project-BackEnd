using MediatR;

namespace Femora.Application.Features.LMS.Courses.Commands.ApproveCourse;

public record ApproveCourseCommand(Guid CourseId, Guid AdminId) : IRequest;
