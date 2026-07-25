using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface ILessonIndexingRepository
{
    Task<Guid> UploadAndIndexLessonResourceAsync(Guid lessonId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task ReindexLessonResourceAsync(Guid lessonResourceId, CancellationToken cancellationToken = default);
    Task SeedIndexedContentAsync(Guid lessonId, string sampleContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recovery for "Storage quota has been exceeded" on the Azure Search free tier:
    /// wipes the whole lesson-chunks index (reclaiming all storage - old chunks from
    /// past dev-time DB resets have nowhere else to go since dropping the SQL DB never
    /// touches this separate Azure resource), clears out the stale seed-content
    /// LessonResource rows, then re-runs the seed video transcript indexing fresh for
    /// every video lesson currently in the DB. Returns how many lessons were re-indexed
    /// and how many failed.
    /// </summary>
    Task<(int Succeeded, int Failed)> ResetAndReindexAllVideoLessonsAsync(CancellationToken cancellationToken = default);
}
