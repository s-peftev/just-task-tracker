using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.DTOs.Attachments;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardTaskAttachmentReadModelMappings
{
    public static BoardTaskAttachmentDto ToDto(
        this BoardTaskAttachmentReadModel attachment,
        IProfilePhotoService profilePhotoService) =>
        new(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            attachment.UploadedBy.ToDto(profilePhotoService));
}
