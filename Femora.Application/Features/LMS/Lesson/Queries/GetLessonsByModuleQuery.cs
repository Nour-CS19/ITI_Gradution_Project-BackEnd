using Femora.Application.Features.LMS.Lesson.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Queries;

public record GetLessonsByModuleQuery(Guid ModuleId)
    : IRequest<List<LessonDto>>;