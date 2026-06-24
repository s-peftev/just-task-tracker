using JustTaskTracker.Application.Common.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class BlobStorageSettings
{
    public TaskAttachmentsStorageOptions? TaskAttachments { get; set; }

    public ProfilePhotosStorageOptions? ProfilePhotos { get; set; }

    public void Validate()
    {
        var section = ConfigSections.BlobStorage;

        if (TaskAttachments is null)
            throw new InvalidOperationException($"{section}:TaskAttachments is not configured.");

        if (ProfilePhotos is null)
            throw new InvalidOperationException($"{section}:ProfilePhotos is not configured.");

        TaskAttachments.Validate($"{section}:TaskAttachments");
        ProfilePhotos.Validate($"{section}:ProfilePhotos");
    }
}

public class TaskAttachmentsStorageOptions
{
    public required string ContainerName { get; set; }

    public required string ActiveFolder { get; set; }

    public required string DeletedFolder { get; set; }

    public string BuildActiveBlobName(Guid boardTaskId, Guid blobId) =>
        $"{Normalize(ActiveFolder)}/{boardTaskId}/{blobId}";

    public string ToDeletedBlobName(string activeBlobName)
    {
        var activePrefix = $"{Normalize(ActiveFolder)}/";

        if (!activeBlobName.StartsWith(activePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Blob '{activeBlobName}' is not under active folder.");

        return $"{Normalize(DeletedFolder)}/{activeBlobName[activePrefix.Length..]}";
    }

    internal void Validate(string sectionPath)
    {
        if (string.IsNullOrWhiteSpace(ContainerName))
            throw new InvalidOperationException($"{sectionPath}:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(ActiveFolder))
            throw new InvalidOperationException($"{sectionPath}:ActiveFolder is not configured.");

        if (string.IsNullOrWhiteSpace(DeletedFolder))
            throw new InvalidOperationException($"{sectionPath}:DeletedFolder is not configured.");
    }

    private static string Normalize(string folder) =>
        folder.Trim().Trim('/');
}

public class ProfilePhotosStorageOptions
{
    public required string ContainerName { get; set; }

    public required string OriginalsFolder { get; set; }

    public required string ThumbnailsFolder { get; set; }

    public string BuildOriginalBlobName(Guid userId) =>
        $"{Normalize(OriginalsFolder)}/{userId}";

    public string BuildThumbnailBlobName(Guid userId) =>
        $"{Normalize(ThumbnailsFolder)}/{userId}";

    internal void Validate(string sectionPath)
    {
        if (string.IsNullOrWhiteSpace(ContainerName))
            throw new InvalidOperationException($"{sectionPath}:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(OriginalsFolder))
            throw new InvalidOperationException($"{sectionPath}:OriginalsFolder is not configured.");

        if (string.IsNullOrWhiteSpace(ThumbnailsFolder))
            throw new InvalidOperationException($"{sectionPath}:ThumbnailsFolder is not configured.");
    }

    private static string Normalize(string folder) =>
        folder.Trim().Trim('/');
}
