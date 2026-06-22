namespace JustTaskTracker.Application.Common.Options;

public class BlobStorageSettings
{
    public TaskAttachmentsStorageOptions TaskAttachments { get; set; } = new();

    public ProfilePhotosStorageOptions ProfilePhotos { get; set; } = new();

    public void Validate()
    {
        var attachmentsSection = "BlobStorageContainers:TaskAttachments";
        var profilePhotosSection = "BlobStorageContainers:TaskAttachments";

        if (string.IsNullOrWhiteSpace(TaskAttachments.ContainerName))
            throw new InvalidOperationException($"{attachmentsSection}:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(TaskAttachments.ActiveFolder))
            throw new InvalidOperationException($"{attachmentsSection}:ActiveFolder is not configured.");

        if (string.IsNullOrWhiteSpace(TaskAttachments.DeletedFolder))
            throw new InvalidOperationException($"{attachmentsSection}:DeletedFolder is not configured.");

        if (string.IsNullOrWhiteSpace(ProfilePhotos.ContainerName))
            throw new InvalidOperationException($"{profilePhotosSection}:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(ProfilePhotos.OriginalsFolder))
            throw new InvalidOperationException($"{profilePhotosSection}:OriginalsFolder is not configured.");

        if (string.IsNullOrWhiteSpace(ProfilePhotos.ThumbnailsFolder))
            throw new InvalidOperationException($"{profilePhotosSection}:ThumbnailsFolder is not configured.");
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

public class ProfilePhotosStorageOptions
{
    public string ContainerName { get; set; } = string.Empty;
    public string OriginalsFolder { get; set; } = string.Empty;
    public string ThumbnailsFolder { get; set; } = string.Empty;

    public string BuildOriginalBlobName(Guid userId) =>
        $"{Normalize(OriginalsFolder)}/{userId}";

    public string BuildThumbnailBlobName(Guid userId) =>
        $"{Normalize(ThumbnailsFolder)}/{userId}";

    private static string Normalize(string folder) =>
        folder.Trim().Trim('/');
}