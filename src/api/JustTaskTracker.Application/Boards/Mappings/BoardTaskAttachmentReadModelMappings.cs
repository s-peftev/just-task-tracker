using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Attachments;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardTaskAttachmentReadModelMappings
{
    public static BoardTaskAttachmentDto ToDto(
        this BoardTaskAttachmentReadModel attachment,
        Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            attachment.UploadedBy.ToDto(profilePhotoUrlResolver));
}
