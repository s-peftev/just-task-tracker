using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.Domain.Boards.Errors;

public static class BoardsErrors
{
    public static readonly Error Archived = new(
        nameof(Archived),
        ErrorType.Conflict,
        ["This board is archived and cannot be modified."]);

    public static readonly Error ReExportNotAllowed = new(
        nameof(ReExportNotAllowed),
        ErrorType.Conflict,
        ["The board must be archived and exported before requesting a new export."]);

    public static readonly Error ReExportAlreadyRequested = new(
        nameof(ReExportAlreadyRequested),
        ErrorType.Conflict,
        ["A re-export has already been requested for this board."]);

    public static readonly Error ExportInfoNotFound = new(
        nameof(ExportInfoNotFound),
        ErrorType.Conflict,
        ["Export metadata is not available for this board."]);

    public static readonly Error ExportNotCompleted = new(
        nameof(ExportNotCompleted),
        ErrorType.Conflict,
        ["The board export is not ready for download yet."]);

    public static readonly Error ArchiveFileNotFound = new(
        nameof(ArchiveFileNotFound),
        ErrorType.NotFound,
        ["The board archive file is not available."]);

    public static readonly Error ReExportOptionsUnchanged = new(
        nameof(ReExportOptionsUnchanged),
        ErrorType.Validation,
        ["Export options must differ from the current export."]);

    public static readonly Error ExportStatusSubscribeNotAllowed = new(
        nameof(ExportStatusSubscribeNotAllowed),
        ErrorType.Forbidden,
        ["One or more boards are not eligible for export status updates."]);
}
