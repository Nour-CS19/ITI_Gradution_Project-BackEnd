using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.DTOs;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Femora.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class LessonIndexingRepository : ILessonIndexingRepository
{
    private readonly IAppDbContext _db;
    private readonly IBlobStorageRepository _blobStorage;
    private readonly ITextExtractionRepository _textExtraction;
    private readonly ITextChunkerRepository _textChunker;
    private readonly IEmbeddingRepository _embedding;
    private readonly ISearchIndexRepository _searchIndex;

    private const string LessonResourcesFolder = "lesson-resources";
    private const int ChunkSize = 300;
    private const int ChunkOverlap = 50;

    public LessonIndexingRepository(
        IAppDbContext db,
        IBlobStorageRepository blobStorage,
        ITextExtractionRepository textExtraction,
        ITextChunkerRepository textChunker,
        IEmbeddingRepository embedding,
        ISearchIndexRepository searchIndex)
    {
        _db = db;
        _blobStorage = blobStorage;
        _textExtraction = textExtraction;
        _textChunker = textChunker;
        _embedding = embedding;
        _searchIndex = searchIndex;
    }

    public async Task<Guid> UploadAndIndexLessonResourceAsync(Guid lessonId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await fileStream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        var blobUrl = await _blobStorage.UploadFileAsync(buffer, fileName, contentType, LessonResourcesFolder, cancellationToken);

        var lessonResource = new LessonResource
        {
            LessonId = lessonId,
            FileName = fileName,
            BlobUrl = blobUrl,
            ContentType = contentType,
            Status = LessonIndexingStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        _db.LessonResources.Add(lessonResource);
        await _db.SaveChangesAsync(cancellationToken);

        buffer.Position = 0;
        await RunPipelineAsync(lessonResource, buffer, cancellationToken);

        return lessonResource.Id;
    }

    public async Task ReindexLessonResourceAsync(Guid lessonResourceId, CancellationToken cancellationToken = default)
    {
        var lessonResource = await _db.LessonResources
            .FirstOrDefaultAsync(r => r.Id == lessonResourceId, cancellationToken)
            ?? throw new KeyNotFoundException($"LessonResource '{lessonResourceId}' not found.");

        var blobName = ExtractBlobName(lessonResource.BlobUrl);
        using var stream = await _blobStorage.DownloadFileAsync(blobName, cancellationToken);
        await _searchIndex.DeleteChunksByLessonResourceIdAsync(lessonResource.Id, cancellationToken);
        await RunPipelineAsync(lessonResource, stream, cancellationToken);
    }

    private async Task RunPipelineAsync(LessonResource resource, Stream fileStream, CancellationToken cancellationToken)
    {
        try
        {
            resource.Status = LessonIndexingStatus.Processing;
            resource.ErrorMessage = null;
            await _db.SaveChangesAsync(cancellationToken);

            var text = await ExtractTextAsync(resource, fileStream, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("No text could be extracted from the document.");

            var chunks = _textChunker.ChunkText(text, ChunkSize, ChunkOverlap);
            if (chunks.Count == 0)
                throw new InvalidOperationException("Text chunking produced no chunks.");

            var embeddings = await _embedding.GenerateEmbeddingsAsync(chunks.Select(c => c.Content), cancellationToken);

            await _searchIndex.EnsureIndexExistsAsync(cancellationToken);

            var documents = chunks.Zip(embeddings, (chunk, embedding) => new LessonChunkDocument
            {
                Id = $"{resource.Id}-{chunk.ChunkIndex}",
                LessonResourceId = resource.Id,
                LessonId = resource.LessonId,
                ChunkIndex = chunk.ChunkIndex,
                Content = chunk.Content,
                Embedding = embedding
            });

            await _searchIndex.UploadChunksAsync(documents, cancellationToken);

            resource.Status = LessonIndexingStatus.Indexed;
            resource.ChunkCount = chunks.Count;
            resource.IndexedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            resource.Status = LessonIndexingStatus.Failed;
            resource.ErrorMessage = ex.Message;
        }
        finally
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private Task<string> ExtractTextAsync(LessonResource resource, Stream fileStream, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(resource.FileName)?.ToLowerInvariant() ?? string.Empty;
        var isDocx = extension == ".docx"
            || string.Equals(resource.ContentType, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);

        if (isDocx)
            return _textExtraction.ExtractTextFromDocxAsync(fileStream, cancellationToken);

        var isPdf = extension == ".pdf" || string.Equals(resource.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        if (isPdf)
            return _textExtraction.ExtractTextFromPdfAsync(fileStream, cancellationToken);

        var isVideo = VideoExtensions.Contains(extension)
            || (resource.ContentType?.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ?? false);
        if (isVideo)
            // Same pipeline as PDF/DOCX from here on: the Whisper transcript
            // gets chunked, embedded and uploaded to the search index below,
            // so lesson videos become searchable/RAG-able exactly like text
            // resources - no separate storage or query path needed.
            return _textExtraction.ExtractTextFromVideoAsync(fileStream, resource.FileName, cancellationToken);

        throw new InvalidOperationException(
            $"Unsupported lesson resource file type \"{extension}\" ({resource.ContentType}). Only .pdf, .docx and video files are supported for indexing.");
    }

    // Whisper only accepts these container/extensions as input - .mov, .m4v and
    // .mpg are NOT among them (even though they're valid video files), so accepting
    // them here would pass the size check above and still fail deep in the Azure
    // SDK with a confusing format error. .mov in particular is the default export
    // format on iPhone, so this is worth calling out explicitly to instructors.
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".mpeg", ".mpga"
    };

    private static string ExtractBlobName(string blobUrl)
    {
        var segments = new Uri(blobUrl).AbsolutePath.TrimStart('/').Split('/', 2);
        var raw = segments.Length == 2 ? segments[1] : segments[0];
        return Uri.UnescapeDataString(raw);
    }

    /// <summary>
    /// Seeds indexed content directly into the search index (used for demo/test data).
    /// Creates a LessonResource record and uploads sample chunks with embeddings.
    /// </summary>
    public async Task SeedIndexedContentAsync(Guid lessonId, string sampleContent, CancellationToken cancellationToken = default)
    {
        LessonResource? lessonResource = null;
        try
        {
            // Ensure the search index exists
            await _searchIndex.EnsureIndexExistsAsync(cancellationToken);

            // Create a LessonResource record for tracking. Starts as Processing -
            // it only becomes Indexed once chunks are actually confirmed uploaded
            // to Azure AI Search below (previously this was set to Indexed here,
            // before any chunking/embedding/upload happened, so a failure further
            // down left a row that *said* Indexed with ChunkCount=0 and nothing in
            // the search index - which is exactly what causes downstream lookups
            // like SummarizeLesson to fail with ContentNotIndexedException even
            // though seeding appeared to succeed).
            lessonResource = new LessonResource
            {
                LessonId = lessonId,
                FileName = "seed-content.txt",
                BlobUrl = "seed://content",
                ContentType = "text/plain",
                Status = LessonIndexingStatus.Processing,
                UploadedAt = DateTime.UtcNow,
                ChunkCount = 0
            };

            _db.LessonResources.Add(lessonResource);
            await _db.SaveChangesAsync(cancellationToken);

            // Chunk the sample content
            var chunks = _textChunker.ChunkText(sampleContent, ChunkSize, ChunkOverlap);
            if (chunks.Count == 0)
                throw new InvalidOperationException("Sample content chunking produced no chunks.");

            // Generate embeddings for each chunk
            var embeddings = await _embedding.GenerateEmbeddingsAsync(chunks.Select(c => c.Content), cancellationToken);

            // Create documents with embeddings
            var documents = chunks.Zip(embeddings, (chunk, embedding) => new LessonChunkDocument
            {
                Id = $"{lessonResource.Id}-{chunk.ChunkIndex}",
                LessonResourceId = lessonResource.Id,
                LessonId = lessonId,
                ChunkIndex = chunk.ChunkIndex,
                Content = chunk.Content,
                Embedding = embedding
            });

            // Upload to search index
            await _searchIndex.UploadChunksAsync(documents, cancellationToken);

            // Only now - after chunks are confirmed uploaded - mark this resource Indexed.
            lessonResource.Status = LessonIndexingStatus.Indexed;
            lessonResource.ChunkCount = chunks.Count;
            lessonResource.IndexedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LessonIndexingRepository] Failed to seed indexed content for lesson {lessonId}: {ex}");

            if (lessonResource is not null)
            {
                try
                {
                    lessonResource.Status = LessonIndexingStatus.Failed;
                    lessonResource.ErrorMessage = ex.Message;
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // Best-effort - don't let a failure to record the failure mask the original exception.
                }
            }

            throw;
        }
    }

    public async Task<(int Succeeded, int Failed)> ResetAndReindexAllVideoLessonsAsync(CancellationToken cancellationToken = default)
    {
        // 1. Reclaim the exhausted quota entirely - deletes the index and recreates it empty.
        await _searchIndex.ResetIndexAsync(cancellationToken);

        // 2. Clear out the stale seed-content LessonResource rows so re-seeding below
        // doesn't just create yet another duplicate generation of them - real
        // instructor-uploaded resources (real BlobUrl, not "seed://content") are left alone.
        var staleSeedResources = await _db.LessonResources
            .Where(r => r.BlobUrl == "seed://content")
            .ToListAsync(cancellationToken);
        foreach (var stale in staleSeedResources)
            _db.LessonResources.Remove(stale);
        await _db.SaveChangesAsync(cancellationToken);

        // 3. Re-run the seed transcript indexing for every video lesson currently in the DB.
        var videoLessons = await _db.Lessons
            .Include(l => l.Module).ThenInclude(m => m.Course)
            .Where(l => l.Type == LessonType.Video)
            .ToListAsync(cancellationToken);

        int succeeded = 0, failed = 0;
        foreach (var lesson in videoLessons)
        {
            try
            {
                var course = lesson.Module?.Course;
                if (course is null) { failed++; continue; }

                var transcript = CourseSeeder.BuildVideoTranscript(
                    course.Category, course.Title, course.Description ?? string.Empty, lesson.Module!.OrderIndex - 1);

                await SeedIndexedContentAsync(lesson.Id, transcript, cancellationToken);
                succeeded++;
            }
            catch (Exception ex)
            {
                failed++;
                Console.Error.WriteLine($"[LessonIndexingRepository] Failed to re-index video lesson {lesson.Id}: {ex.Message}");
            }

            await Task.Delay(300, cancellationToken);
        }

        return (succeeded, failed);
    }
}