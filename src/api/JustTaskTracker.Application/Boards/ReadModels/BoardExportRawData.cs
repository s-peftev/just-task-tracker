using JustTaskTracker.Domain.Boards.DTOs.Archiving;

namespace JustTaskTracker.Application.Boards.ReadModels;

/// <summary>
/// Intermediate read model produced by the persistence layer for board export.
/// Attachments carry <see cref="BoardExportRawAttachmentData.BlobName"/> instead of SAS URLs;
/// the Application layer enriches them with time-limited download links before returning to the Function.
/// </summary>
public record BoardExportRawData(
    BoardExportBoardDto Board,
    IReadOnlyList<BoardExportRawColumnData> Columns,
    IReadOnlyList<BoardExportMemberDto>? Members);

public record BoardExportRawColumnData(
    Guid Id,
    string Name,
    int Position,
    IReadOnlyList<BoardExportRawTaskData> Tasks);

public record BoardExportRawTaskData(
    Guid Id,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    BoardExportUserDto Reporter,
    BoardExportUserDto? Assignee,
    string? Description,
    IReadOnlyList<BoardExportCommentDto>? Comments,
    IReadOnlyList<BoardExportRawAttachmentData>? Attachments);

public record BoardExportRawAttachmentData(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    int Position,
    DateTime CreatedAtUtc,
    BoardExportUserDto UploadedBy,
    string BlobName);
