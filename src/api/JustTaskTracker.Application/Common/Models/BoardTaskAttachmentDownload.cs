namespace JustTaskTracker.Application.Common.Models;

/// <summary>
/// Attachment file payload returned by <see cref="Boards.Commands.Attachments.DownloadBoardTaskAttachmentCommand"/>.
/// </summary>
/// <param name="OriginalFileName">Client-facing download name from attachment metadata.</param>
/// <param name="Blob">Stream and headers from blob storage; pass directly to ASP.NET file result helpers.</param>
public record BoardTaskAttachmentDownload(
    string OriginalFileName,
    BlobContent Blob);
