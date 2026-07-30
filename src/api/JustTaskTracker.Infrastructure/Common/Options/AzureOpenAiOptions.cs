using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ChatDeploymentName { get; set; } = string.Empty;

    public void Validate()
    {
        var section = ConfigSections.AzureOpenAi;

        if (string.IsNullOrWhiteSpace(Endpoint))
            throw new InvalidOperationException($"{section}:{nameof(Endpoint)} is not configured.");

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
            throw new InvalidOperationException($"{section}:{nameof(Endpoint)} must be a valid absolute URI.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException($"{section}:{nameof(ApiKey)} is not configured.");

        if (string.IsNullOrWhiteSpace(ChatDeploymentName))
            throw new InvalidOperationException($"{section}:{nameof(ChatDeploymentName)} is not configured.");
    }
}
