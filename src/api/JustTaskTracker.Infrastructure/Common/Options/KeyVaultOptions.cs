using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class KeyVaultOptions
{
    public string Uri { get; set; } = string.Empty;

    public void Validate()
    {
        var section = ConfigSections.KeyVault;

        if (string.IsNullOrWhiteSpace(Uri))
            throw new InvalidOperationException($"{section}:{nameof(Uri)} is not configured.");

        if (!System.Uri.TryCreate(Uri, UriKind.Absolute, out _))
            throw new InvalidOperationException($"{section}:{nameof(Uri)} must be a valid absolute URI.");
    }
}
