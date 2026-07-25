using System;
using System.Collections.Generic;
using MediatR;

namespace Femora.Application.Features.LMS.Lesson.Queries.GetLessonResources;

public record GetLessonResourcesQuery(Guid LessonId) : IRequest<List<LessonResourceStatusDto>>;

public record LessonResourceStatusDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ChunkCount { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime UploadedAt { get; init; }
    public DateTime? IndexedAt { get; init; }
}
