using System.Text.Json;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

namespace JustTaskTracker.WebUI.Services.Boards.Actions;

internal static class BoardActionPayloadParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static BoardActionPayload Parse(BoardActionNotificationType type, JsonElement payload) =>
        type switch
        {
            BoardActionNotificationType.BoardRenamed =>
                payload.Deserialize<BoardRenamedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.ColumnCreated =>
                payload.Deserialize<ColumnCreatedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.ColumnRenamed =>
                payload.Deserialize<ColumnRenamedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.ColumnDeleted =>
                payload.Deserialize<ColumnDeletedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.ColumnsReordered =>
                payload.Deserialize<ColumnsReorderedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.TaskCreated =>
                payload.Deserialize<TaskCreatedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.TaskRenamed =>
                payload.Deserialize<TaskRenamedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.TaskDeleted =>
                payload.Deserialize<TaskDeletedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.TasksReordered =>
                payload.Deserialize<TasksReorderedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.CommentCreated =>
                payload.Deserialize<CommentCreatedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.CommentDeleted =>
                payload.Deserialize<CommentDeletedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.AttachmentUploaded =>
                payload.Deserialize<AttachmentUploadedPayload>(Options)
                ?? throw CreateParseException(type),

            BoardActionNotificationType.AttachmentDeleted =>
                payload.Deserialize<AttachmentDeletedPayload>(Options)
                ?? throw CreateParseException(type),

            _ => throw new NotSupportedException($"Board action notification type '{type}' is not supported."),
        };

    private static InvalidOperationException CreateParseException(BoardActionNotificationType type) =>
        new($"Failed to deserialize board action payload for type '{type}'.");
}
