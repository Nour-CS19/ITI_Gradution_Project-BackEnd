using MediatR;
using Femora.Domain.Enums;

namespace Femora.Application.Features.LMS.Courses.Commands;

public record UpdateCourseCommand(
    Guid CourseId,
    string Title,
    string? Description,
    decimal Price,
    string Category,
    CourseLevel Level,
    string Language,
    string? ThumbnailUrl
) : IRequest;