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
        ["The board must be archived and serialized before requesting a new export."]);

    public static readonly Error ReExportAlreadyRequested = new(
        nameof(ReExportAlreadyRequested),
        ErrorType.Conflict,
        ["A re-export has already been requested for this board."]);

    public static readonly Error ExportNotCompleted = new(
        nameof(ExportNotCompleted),
        ErrorType.Conflict,
        ["A new export can only be requested after the current export has completed."]);

    public static readonly Error SerializationInfoNotFound = new(
        nameof(SerializationInfoNotFound),
        ErrorType.Conflict,
        ["Serialization metadata is not available for this board."]);

    public static readonly Error ReExportOptionsUnchanged = new(
        nameof(ReExportOptionsUnchanged),
        ErrorType.Validation,
        ["Export options must differ from the current export."]);
}
