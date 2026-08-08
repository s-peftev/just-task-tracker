namespace JustTaskTracker.Archival.Functions.Options;

public class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string Uri { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Uri))
            throw new InvalidOperationException($"{SectionName}:{nameof(Uri)} is not configured.");

        if (!System.Uri.TryCreate(Uri, UriKind.Absolute, out _))
            throw new InvalidOperationException($"{SectionName}:{nameof(Uri)} must be a valid absolute URI.");
    }
}
