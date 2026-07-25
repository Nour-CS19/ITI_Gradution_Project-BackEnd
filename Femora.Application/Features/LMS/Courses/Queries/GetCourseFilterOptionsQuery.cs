using Femora.Application.Features.LMS.Courses.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Courses.Queries;

public sealed record GetCourseFilterOptionsQuery : IRequest<CourseFilterOptionsDto>;

