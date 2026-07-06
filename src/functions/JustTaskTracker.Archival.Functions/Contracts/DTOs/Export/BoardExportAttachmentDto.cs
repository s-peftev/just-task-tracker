using System.Text.Json.Serialization;

namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportAttachmentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int Position,
    DateTime CreatedAtUtc,
    BoardExportUserDto UploadedBy,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)] string BlobName,
    string? ArchiveRelativePath = null);
