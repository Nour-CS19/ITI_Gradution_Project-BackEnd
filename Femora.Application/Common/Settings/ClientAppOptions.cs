namespace Femora.Application.Common.Settings;

public class ClientAppOptions
{
    public const string SectionName = "ClientApp";

    /// <summary>Base URL of the Angular frontend, used to build links sent by email (e.g. reset-password).</summary>
    public string BaseUrl { get; set; } = "http://localhost:4200";
}
