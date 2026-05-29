namespace JustTaskTracker.WebUI.Services.Configuration;

public sealed class ApiClientOptions
{
    public const string SectionName = "Api";
    public string BaseUrl { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = [];
}
