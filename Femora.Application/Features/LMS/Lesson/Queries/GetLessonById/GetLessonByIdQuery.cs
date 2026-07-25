using Femora.Application.Features.LMS.Lesson.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Queries.GetLessonById;

public record GetLessonByIdQuery(Guid LessonId) : IRequest<LessonDetailsDto>;
