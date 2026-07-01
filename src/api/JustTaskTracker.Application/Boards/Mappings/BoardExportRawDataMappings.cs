using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Domain.Boards.DTOs.Archiving;
using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardExportRawDataMappings
{
    public static BoardExportDataDto ToDto(
        this BoardExportRawData raw,
        BoardExportOptions options,
        IBlobStorageService blobStorageService,
        string attachmentsContainerName,
        TimeSpan attachmentSasValidity) =>
        new(
            raw.Board,
            options,
            DateTime.UtcNow,
            raw.Columns
                .Select(c => c.ToDto(blobStorageService, attachmentsContainerName, attachmentSasValidity))
                .ToList(),
            raw.Members);

    public static BoardExportColumnDto ToDto(
        this BoardExportRawColumnData column,
        IBlobStorageService blobStorageService,
        string attachmentsContainerName,
        TimeSpan attachmentSasValidity) =>
        new(
            column.Id,
            column.Name,
            column.Position,
            column.Tasks
                .Select(t => t.ToDto(blobStorageService, attachmentsContainerName, attachmentSasValidity))
                .ToList());

    public static BoardExportTaskDto ToDto(
        this BoardExportRawTaskData task,
        IBlobStorageService blobStorageService,
        string attachmentsContainerName,
        TimeSpan attachmentSasValidity) =>
        new(
            task.Id,
            task.Title,
            task.Position,
            task.CreatedAtUtc,
            task.LastModifiedAtUtc,
            task.Reporter,
            task.Assignee,
            task.Description,
            task.Comments,
            task.Attachments?
                .Select(a => a.ToDto(blobStorageService, attachmentsContainerName, attachmentSasValidity))
                .ToList());

    public static BoardExportAttachmentDto ToDto(
        this BoardExportRawAttachmentData attachment,
        IBlobStorageService blobStorageService,
        string attachmentsContainerName,
        TimeSpan attachmentSasValidity)
    {
        var expiresAt = DateTime.UtcNow.Add(attachmentSasValidity);

        var sasUri = blobStorageService.GenerateReadSasUri(
            attachmentsContainerName,
            attachment.BlobName,
            attachmentSasValidity);

        return new BoardExportAttachmentDto(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            attachment.UploadedBy,
            sasUri,
            expiresAt);
    }
}
