using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Application.Common.Images;
using JustTaskTracker.Application.Common.Options;

namespace JustTaskTracker.Application.Users.ProfilePhotos;

internal sealed class ProfilePhotoService(
    IBlobStorageService blobStorageService,
    IImageProcessor imageProcessor,
    BlobStorageSettings blobStorageSettings,
    ProfilePhotoProcessingSettings processingSettings) : IProfilePhotoService
{
    private const string WebpContentType = "image/webp";

    private readonly string _containerName = blobStorageSettings.ProfilePhotos.ContainerName
        is { Length: > 0 } containerName
            ? containerName
            : throw new InvalidOperationException(
                $"{nameof(blobStorageSettings.ProfilePhotos.ContainerName)} is not configured.");

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

    public async Task UploadPhotoAsync(Guid userId, Stream source, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentNullException.ThrowIfNull(source);

        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, ct);

        if (buffer.Length == 0)
            throw new InvalidOperationException("Image stream is empty.");

        buffer.Position = 0;

        var specOriginal = ToSpec(processingSettings.Originals!);
        var specThumbnail = ToSpec(processingSettings.Thumbnails!);

        await using var originalStream = await imageProcessor.ProcessToWebpAsync(buffer, specOriginal, ct);

        buffer.Position = 0;

        await using var thumbnailStream = await imageProcessor.ProcessToWebpAsync(buffer, specThumbnail, ct);

        var originalBlobName = blobStorageSettings.ProfilePhotos.BuildOriginalBlobName(userId);
        var thumbnailBlobName = blobStorageSettings.ProfilePhotos.BuildThumbnailBlobName(userId);

        await Task.WhenAll(
            blobStorageService.UploadAsync(_containerName, originalBlobName, originalStream, WebpContentType, ct),
            blobStorageService.UploadAsync(_containerName, thumbnailBlobName, thumbnailStream, WebpContentType, ct));
    }

    private static ImageProcessingSpec ToSpec(ProfilePhotoOutputSettings settings) =>
        new(settings.Width, settings.Height, settings.WebpQuality);
}
