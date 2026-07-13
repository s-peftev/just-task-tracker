using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class AzureAdOptions
{
    public string Instance { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

    public void Validate()
    {
        var section = ConfigSections.AzureAd;

        if (string.IsNullOrWhiteSpace(Instance))
            throw new InvalidOperationException($"{section}:Instance is not configured.");

        if (string.IsNullOrWhiteSpace(Domain))
            throw new InvalidOperationException($"{section}:Domain is not configured.");

        if (string.IsNullOrWhiteSpace(TenantId))
            throw new InvalidOperationException($"{section}:TenantId is not configured.");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException($"{section}:ClientId is not configured.");
    }
}
