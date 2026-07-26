namespace Femora.Infrastructure.Options;

public class AzureOpenAIOptions
{
    public const string SectionName = "OpenAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChatDeploymentName { get; set; } = string.Empty;
    public string EmbeddingDeploymentName { get; set; } = string.Empty;
    /// <summary>
    /// Whisper deployment used to transcribe lesson video audio so the
    /// resulting transcript can be chunked/embedded/indexed exactly like
    /// PDF/DOCX lesson resources.
    /// </summary>
    public string WhisperDeploymentName { get; set; } = "whisper";
}
