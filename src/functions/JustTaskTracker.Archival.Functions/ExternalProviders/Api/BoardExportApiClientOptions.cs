namespace JustTaskTracker.Archival.Functions.ExternalProviders.Api;

public sealed class BoardExportApiClientOptions
{
    public const string SectionName = "BoardExportApi";

    public required string BaseAddress { get; init; }

    public required string ApiKeyHeaderName { get; init; }

    public required string ApiKey { get; init; }

    public required int RequestTimeoutMinutes { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseAddress))
            throw new InvalidOperationException($"{SectionName}:BaseAddress is not configured.");

        if (string.IsNullOrWhiteSpace(ApiKeyHeaderName))
            throw new InvalidOperationException($"{SectionName}:ApiKeyHeaderName is not configured.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException($"{SectionName}:ApiKey is not configured.");

        if (RequestTimeoutMinutes <= 0)
            throw new InvalidOperationException($"{SectionName}:RequestTimeoutMinutes must be greater than zero.");
    }
}
