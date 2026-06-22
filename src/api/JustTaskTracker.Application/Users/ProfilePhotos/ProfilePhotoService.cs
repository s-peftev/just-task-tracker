using JustTaskTracker.Application.Common.Options;

namespace JustTaskTracker.Application.Users.ProfilePhotos;

internal sealed class ProfilePhotoService(BlobStorageSettings blobStorageSettings) : IProfilePhotoService
{
    public string BuildOriginalUrl(Guid userId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        return blobStorageSettings.ProfilePhotos.BuildOriginalBlobName(userId);
    }

    public string BuildThumbnailUrl(Guid userId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        return blobStorageSettings.ProfilePhotos.BuildThumbnailBlobName(userId);
    }
}
