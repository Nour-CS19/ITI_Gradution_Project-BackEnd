using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.DTOs;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class SearchIndexRepository : ISearchIndexRepository
{
    private const int EmbeddingDimensions = 1536;
    private const string VectorProfileName = "lesson-vector-profile";
    private const string VectorAlgorithmName = "lesson-hnsw";

    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly string _indexName;

    public SearchIndexRepository(IOptions<AzureSearchOptions> options)
    {
        var settings = options.Value;
        _indexName = settings.LessonChunksIndexName;
        var credential = new AzureKeyCredential(settings.ApiKey);
        var endpoint = new Uri(settings.Endpoint);
        _indexClient = new SearchIndexClient(endpoint, credential);
        _searchClient = new SearchClient(endpoint, _indexName, credential);
    }

    public async Task EnsureIndexExistsAsync(CancellationToken cancellationToken = default)
    {
        try { await _indexClient.GetIndexAsync(_indexName, cancellationToken); return; }
        catch (RequestFailedException ex) when (ex.Status == 404) { }

        var index = new SearchIndex(_indexName)
        {
            Fields = new List<SearchField>
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                new SimpleField("lessonResourceId", SearchFieldDataType.String) { IsFilterable = true },
                new SimpleField("lessonId", SearchFieldDataType.String) { IsFilterable = true },
                new SimpleField("chunkIndex", SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true },
                new SearchableField("content"),
                new VectorSearchField("embedding", EmbeddingDimensions, VectorProfileName)
            },
            VectorSearch = new VectorSearch
            {
                Profiles = { new VectorSearchProfile(VectorProfileName, VectorAlgorithmName) },
                Algorithms = { new HnswAlgorithmConfiguration(VectorAlgorithmName) }
            }
        };
        await _indexClient.CreateIndexAsync(index, cancellationToken);
    }

    public async Task UploadChunksAsync(IEnumerable<LessonChunkDocument> chunks, CancellationToken cancellationToken = default)
    {
        var documents = chunks.Select(c => new SearchDocument
        {
            ["id"] = c.Id,
            ["lessonResourceId"] = c.LessonResourceId.ToString(),
            ["lessonId"] = c.LessonId.ToString(),
            ["chunkIndex"] = c.ChunkIndex,
            ["content"] = c.Content,
            ["embedding"] = c.Embedding
        }).ToList();

        if (documents.Count == 0) return;
        await _searchClient.MergeOrUploadDocumentsAsync(documents, cancellationToken: cancellationToken);
    }

    public async Task DeleteChunksByLessonResourceIdAsync(Guid lessonResourceId, CancellationToken cancellationToken = default)
    {
        var ids = new List<string>();
        var response = await _searchClient.SearchAsync<SearchDocument>("*", new SearchOptions
        {
            Filter = $"lessonResourceId eq '{lessonResourceId}'",
            Select = { "id" },
            Size = 1000
        }, cancellationToken);

        await foreach (var result in response.Value.GetResultsAsync())
            if (result.Document.TryGetValue("id", out var id) && id is string sid) ids.Add(sid);

        if (ids.Count == 0) return;
        await _searchClient.DeleteDocumentsAsync(ids.Select(id => new SearchDocument { ["id"] = id }), cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<LessonChunkSearchResult>> SearchAsync(float[] queryEmbedding, int top = 5, Guid? lessonId = null, CancellationToken cancellationToken = default)
    {
        var options = new SearchOptions
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(queryEmbedding) { KNearestNeighborsCount = top, Fields = { "embedding" } } },
                // Without this, the lessonId filter below is applied AFTER the k-nearest-neighbors
                // search runs across the whole index, so a lesson's genuinely relevant chunks can be
                // squeezed out by closer vectors that belong to *other* lessons, and this lesson's
                // filtered results come back empty or off-topic - PreFilter runs the KNN search only
                // over documents that already match the lessonId filter.
                FilterMode = VectorFilterMode.PreFilter,
            },
            Size = top
        };
        if (lessonId.HasValue) options.Filter = $"lessonId eq '{lessonId.Value}'";

        // Passing null as the search text (instead of "*") runs a pure vector search.
        // "*" would trigger hybrid search (text + vector, fused via reciprocal rank
        // fusion) - every chunk matches "*" so the text side just injects an
        // arbitrary tie-broken rank into that fusion, which can knock a genuinely
        // relevant chunk out of the top results for no real reason.
        var response = await _searchClient.SearchAsync<SearchDocument>(searchText: null, options, cancellationToken);
        var results = new List<LessonChunkSearchResult>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            var doc = result.Document;
            results.Add(new LessonChunkSearchResult
            {
                Id = doc.TryGetValue("id", out var id) ? id?.ToString() ?? "" : "",
                LessonId = doc.TryGetValue("lessonId", out var lId) && Guid.TryParse(lId?.ToString(), out var pLId) ? pLId : Guid.Empty,
                LessonResourceId = doc.TryGetValue("lessonResourceId", out var lrId) && Guid.TryParse(lrId?.ToString(), out var pLrId) ? pLrId : Guid.Empty,
                ChunkIndex = doc.TryGetValue("chunkIndex", out var idx) && idx is int ci ? ci : 0,
                Content = doc.TryGetValue("content", out var content) ? content?.ToString() ?? "" : "",
                Score = result.Score ?? 0
            });
        }
        return results;
    }

    public async Task ResetIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _indexClient.DeleteIndexAsync(_indexName, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Already gone - fine, EnsureIndexExistsAsync below will create it fresh.
        }

        await EnsureIndexExistsAsync(cancellationToken);
    }
}
