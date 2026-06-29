using JustTaskTracker.Application.Common.ExternalProviders;

namespace JustTaskTracker.Application.Common.Models;

/// <summary>
/// Blob payload returned by <see cref="IBlobStorageService.DownloadAsync"/>.
/// </summary>
/// <param name="Content">
/// Readable stream backed by blob storage. When returned to ASP.NET via <c>Results.File</c> or
/// <c>FileStreamResult</c>, the framework disposes it after the response is sent. Otherwise the caller
/// must dispose <paramref name="Content"/> (for example in a <c>catch</c> block if an error occurs
/// before the stream is handed off).
/// </param>
/// <param name="ContentType">MIME type stored on the blob.</param>
/// <param name="ContentLength">Blob size in bytes.</param>
public record BlobContent(
    Stream Content,
    string ContentType,
    long ContentLength);
