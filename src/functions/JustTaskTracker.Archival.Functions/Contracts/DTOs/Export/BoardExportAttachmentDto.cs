namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public sealed record BoardExportAttachmentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int Position,
    DateTime CreatedAtUtc,
    BoardExportUserDto UploadedBy,
    Uri DownloadUrl,
    DateTime DownloadUrlExpiresAtUtc);
