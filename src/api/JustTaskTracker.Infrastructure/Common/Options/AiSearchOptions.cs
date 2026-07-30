using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class AiSearchOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string IndexName { get; set; } = string.Empty;

    public string SemanticConfigurationName { get; set; } = string.Empty;

    public string VectorFieldName { get; set; } = string.Empty;

    public string ContentFieldName { get; set; } = string.Empty;

    public int TopK { get; set; }

    public void Validate()
    {
        var section = ConfigSections.AiSearch;

        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException($"{section}:{nameof(Endpoint)} is not configured.");

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
            throw new InvalidOperationException($"{section}:{nameof(Endpoint)} must be a valid absolute URI.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException($"{section}:{nameof(ApiKey)} is not configured.");

        if (string.IsNullOrWhiteSpace(IndexName))
            throw new InvalidOperationException($"{section}:{nameof(IndexName)} is not configured.");

        if (string.IsNullOrWhiteSpace(SemanticConfigurationName))
            throw new InvalidOperationException($"{section}:{nameof(SemanticConfigurationName)} is not configured.");

        if (string.IsNullOrWhiteSpace(VectorFieldName))
            throw new InvalidOperationException($"{section}:{nameof(VectorFieldName)} is not configured.");

        if (string.IsNullOrWhiteSpace(ContentFieldName))
            throw new InvalidOperationException($"{section}:{nameof(ContentFieldName)} is not configured.");

        if (TopK <= 0)
            throw new InvalidOperationException($"{section}:{nameof(TopK)} must be greater than 0.");
    }
}
