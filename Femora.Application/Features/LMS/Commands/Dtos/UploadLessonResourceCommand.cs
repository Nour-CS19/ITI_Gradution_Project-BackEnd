using MediatR;

namespace Femora.Application.Features.LMS.Commands.UploadLessonResource;

/// <summary>
/// Command carries only serializable data.
/// The Stream is injected by the controller before sending.
/// </summary>
public record UploadLessonResourceCommand : IRequest<UploadLessonResourceResponse>
{
    public Guid LessonId { get; init; }

    // NOT serialized by Swagger — set by controller from IFormFile
    [System.Text.Json.Serialization.JsonIgnore]
    public Stream FileStream { get; init; } = Stream.Null;

    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public record UploadLessonResourceResponse
{
    public Guid LessonResourceId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}