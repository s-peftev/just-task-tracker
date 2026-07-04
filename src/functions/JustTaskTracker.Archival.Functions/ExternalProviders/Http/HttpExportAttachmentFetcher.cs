using JustTaskTracker.Archival.Functions.Abstractions.Archiving;
using JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

namespace JustTaskTracker.Archival.Functions.ExternalProviders.Http;

/// <summary>
/// Downloads attachment content via a time-limited URL provided by the API.
/// The caller is responsible for disposing the returned stream.
/// </summary>
public sealed class HttpExportAttachmentFetcher(HttpClient httpClient) : IExportAttachmentFetcher
{
    public async Task<Stream> DownloadAsync(BoardExportAttachmentDto attachment, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        if (attachment.DownloadUrlExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                $"Attachment {attachment.Id} ('{attachment.OriginalFileName}') download URL expired at {attachment.DownloadUrlExpiresAtUtc:O}.");
        }

        var response = await httpClient.GetAsync(
            attachment.DownloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();

        var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        return buffer;
    }
}
