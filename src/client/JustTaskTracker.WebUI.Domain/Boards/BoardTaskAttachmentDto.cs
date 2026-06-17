using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskAttachmentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int Position,
    DateTime CreatedAtUtc,
    UserDto UploadedBy);
