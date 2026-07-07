using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

namespace JustTaskTracker.WebUI.Services.Boards.Actions;

internal static class BoardActionSyncKey
{
    public static string Resolve(BoardActionNotification notification) =>
        notification.Type switch
        {
            BoardActionNotificationType.BoardRenamed => "board",

            BoardActionNotificationType.ColumnCreated =>
                $"column:{((ColumnCreatedPayload)notification.Payload).ColumnId}",

            BoardActionNotificationType.ColumnRenamed =>
                $"column:{((ColumnRenamedPayload)notification.Payload).ColumnId}",

            BoardActionNotificationType.ColumnDeleted =>
                $"column:{((ColumnDeletedPayload)notification.Payload).ColumnId}",

            BoardActionNotificationType.ColumnsReordered => "board:columns-order",

            BoardActionNotificationType.TaskCreated =>
                $"task:{((TaskCreatedPayload)notification.Payload).BoardTaskId}",

            BoardActionNotificationType.TaskRenamed =>
                $"task:{((TaskRenamedPayload)notification.Payload).BoardTaskId}",

            BoardActionNotificationType.TaskDeleted =>
                $"task:{((TaskDeletedPayload)notification.Payload).BoardTaskId}",

            BoardActionNotificationType.TasksReordered =>
                $"task:{((TasksReorderedPayload)notification.Payload).BoardTaskId}:position",

            BoardActionNotificationType.CommentCreated =>
                $"comment:{((CommentCreatedPayload)notification.Payload).Comment.Id}",

            BoardActionNotificationType.CommentDeleted =>
                $"comment:{((CommentDeletedPayload)notification.Payload).CommentId}",

            BoardActionNotificationType.AttachmentUploaded =>
                $"attachment:{((AttachmentUploadedPayload)notification.Payload).Attachment.Id}",

            BoardActionNotificationType.AttachmentDeleted =>
                $"attachment:{((AttachmentDeletedPayload)notification.Payload).AttachmentId}",

            _ => $"type:{(byte)notification.Type}",
        };
}
