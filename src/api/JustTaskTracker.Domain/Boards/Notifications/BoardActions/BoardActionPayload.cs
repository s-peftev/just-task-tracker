using System.Text.Json.Serialization;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BoardRenamedPayload), "boardRenamed")]
[JsonDerivedType(typeof(ColumnCreatedPayload), "columnCreated")]
[JsonDerivedType(typeof(ColumnRenamedPayload), "columnRenamed")]
[JsonDerivedType(typeof(ColumnDeletedPayload), "columnDeleted")]
[JsonDerivedType(typeof(ColumnsReorderedPayload), "columnsReordered")]
[JsonDerivedType(typeof(TaskCreatedPayload), "taskCreated")]
[JsonDerivedType(typeof(TaskRenamedPayload), "taskRenamed")]
[JsonDerivedType(typeof(TaskDeletedPayload), "taskDeleted")]
[JsonDerivedType(typeof(TasksReorderedPayload), "tasksReordered")]
[JsonDerivedType(typeof(CommentCreatedPayload), "commentCreated")]
[JsonDerivedType(typeof(CommentDeletedPayload), "commentDeleted")]
[JsonDerivedType(typeof(AttachmentUploadedPayload), "attachmentUploaded")]
[JsonDerivedType(typeof(AttachmentDeletedPayload), "attachmentDeleted")]
public abstract record BoardActionPayload;
