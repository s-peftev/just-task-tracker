using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record DeleteColumnRequest(
    DeleteColumnTasksDisposition TasksDisposition,
    Guid? TargetColumnId = null,
    ColumnTaskMovePlacement? MovePlacement = null);
