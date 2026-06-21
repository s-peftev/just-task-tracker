namespace JustTaskTracker.Application.Common.Options;

public class BlobStorageSettings
{
    public TaskAttachmentsStorageOptions TaskAttachments { get; set; } = new();

    public void Validate()
    {
        var section = "BlobStorageContainers:TaskAttachments";

        if (string.IsNullOrWhiteSpace(TaskAttachments.ContainerName))
            throw new InvalidOperationException($"{section}:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(TaskAttachments.ActiveFolder))
            throw new InvalidOperationException($"{section}:ActiveFolder is not configured.");

        if (string.IsNullOrWhiteSpace(TaskAttachments.DeletedFolder))
            throw new InvalidOperationException($"{section}:DeletedFolder is not configured.");
    }
}

public class TaskAttachmentsStorageOptions
{
    public string ContainerName { get; set; } = string.Empty;
    public string ActiveFolder { get; set; } = string.Empty;
    public string DeletedFolder { get; set; } = string.Empty;

    public string BuildActiveBlobName(Guid boardTaskId, Guid blobId) =>
        $"{Normalize(ActiveFolder)}/{boardTaskId}/{blobId}";

    public string ToDeletedBlobName(string activeBlobName)
    {
        var activePrefix = $"{Normalize(ActiveFolder)}/";

        if (!activeBlobName.StartsWith(activePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Blob '{activeBlobName}' is not under active folder.");

        return $"{Normalize(DeletedFolder)}/{activeBlobName[activePrefix.Length..]}";
    }

    private static string Normalize(string folder) =>
        folder.Trim().Trim('/');
}
