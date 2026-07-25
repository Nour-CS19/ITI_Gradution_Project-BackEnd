using MediatR;

namespace Femora.Application.Features.Admin.Commands.ResetAndReindexVideoLessons;

/// <summary>
/// Recovery action for "Storage quota has been exceeded" on the Azure Search free
/// tier: wipes the lesson-chunks index and re-indexes every video lesson currently
/// in the DB from scratch. Admin-only, since it touches the shared search index.
/// </summary>
public record ResetAndReindexVideoLessonsCommand : IRequest<ResetAndReindexVideoLessonsResponse>;

public record ResetAndReindexVideoLessonsResponse
{
    public int Succeeded { get; init; }
    public int Failed { get; init; }
}
