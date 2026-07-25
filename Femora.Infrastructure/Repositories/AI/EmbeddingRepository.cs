using Azure;
using Azure.AI.OpenAI;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositories;

public class EmbeddingRepository : IEmbeddingRepository
{
    private readonly EmbeddingClient _embeddingClient;

    public EmbeddingRepository(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;
        var azureClient = new AzureOpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        _embeddingClient = azureClient.GetEmbeddingClient(settings.EmbeddingDeploymentName);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var result = await _embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats().ToArray();
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        if (textList.Count == 0) return Array.Empty<float[]>();
        var result = await _embeddingClient.GenerateEmbeddingsAsync(textList, cancellationToken: cancellationToken);
        return result.Value.Select(e => e.ToFloats().ToArray()).ToList();
    }
}
