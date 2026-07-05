using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Archiving;
using JustTaskTracker.Domain.Boards.DTOs.Boards;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardExportRawDataMappings
{
    public static BoardExportDataDto ToDto(this BoardExportRawData raw, BoardExportOptions options) =>
        new(
            raw.Board,
            options,
            DateTime.UtcNow,
            raw.Columns.Select(c => c.ToDto()).ToList(),
            raw.Members);

    public static BoardExportColumnDto ToDto(this BoardExportRawColumnData column) =>
        new(
            column.Id,
            column.Name,
            column.Position,
            column.Tasks.Select(t => t.ToDto()).ToList());

    public static BoardExportTaskDto ToDto(this BoardExportRawTaskData task) =>
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
            task.Attachments?.Select(a => a.ToDto()).ToList());

    public static BoardExportAttachmentDto ToDto(this BoardExportRawAttachmentData attachment) =>
        new(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Position,
            attachment.CreatedAtUtc,
            attachment.UploadedBy,
            attachment.BlobName);
}
