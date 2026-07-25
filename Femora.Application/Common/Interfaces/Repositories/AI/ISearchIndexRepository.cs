using Femora.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface ISearchIndexRepository
{
    Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default);
    Task UploadChunksAsync(IEnumerable<LessonChunkDocument> chunks, CancellationToken cancellationToken = default);
    Task DeleteChunksByLessonResourceIdAsync(Guid lessonResourceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LessonChunkSearchResult>> SearchAsync(float[] queryEmbedding, int top = 5, Guid? lessonId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes and recreates the whole lesson-chunks index, reclaiming ALL of its
    /// storage. Use this to recover from "Storage quota has been exceeded" on the
    /// free/basic Azure Search tier - repeated dev-time DB resets + reseeds leave
    /// behind orphaned chunk documents (tied to LessonResource/Lesson ids that no
    /// longer exist) that this index has no other way to clean up, since deleting
    /// the SQL database doesn't touch this separate, persistent Azure resource.
    /// </summary>
    Task ResetIndexAsync(CancellationToken cancellationToken = default);
}
