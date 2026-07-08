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
[JsonDerivedType(typeof(TaskUpdatedPayload), "taskUpdated")]
[JsonDerivedType(typeof(TaskDeletedPayload), "taskDeleted")]
[JsonDerivedType(typeof(TasksReorderedPayload), "tasksReordered")]
[JsonDerivedType(typeof(TaskCommentsCountChangedPayload), "taskCommentsCountChanged")]
[JsonDerivedType(typeof(TaskAttachmentsCountChangedPayload), "taskAttachmentsCountChanged")]
public abstract record BoardActionPayload;
