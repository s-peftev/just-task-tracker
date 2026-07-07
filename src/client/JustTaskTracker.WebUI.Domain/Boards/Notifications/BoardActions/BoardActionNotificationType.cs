namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

public enum BoardActionNotificationType : byte
{
    BoardRenamed = 1,
    ColumnCreated = 2,
    ColumnRenamed = 3,
    ColumnDeleted = 4,
    ColumnsReordered = 5,
    TaskCreated = 6,
    TaskRenamed = 7,
    TaskDeleted = 8,
    TasksReordered = 9,
    CommentCreated = 10,
    CommentDeleted = 11,
    AttachmentUploaded = 12,
    AttachmentDeleted = 13,
}
