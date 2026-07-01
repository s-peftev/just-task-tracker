namespace JustTaskTracker.Archival.Functions.ExternalProviders.Blob;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public required string ArchivesContainerName { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ArchivesContainerName))
            throw new InvalidOperationException($"{SectionName}:{nameof(ArchivesContainerName)} is not configured.");
    }
}
