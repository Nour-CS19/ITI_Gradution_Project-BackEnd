namespace Femora.Infrastructure.Options;

public class AzureSearchOptions
{
    public const string SectionName = "Search";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string LessonChunksIndexName { get; set; } = string.Empty;
}
