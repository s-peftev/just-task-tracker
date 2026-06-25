using JustTaskTracker.Application.Users.ReadModels;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardTaskAttachmentReadModel(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int Position,
    DateTime CreatedAtUtc,
    UserReadModel UploadedBy);
