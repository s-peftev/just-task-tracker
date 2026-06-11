using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Validation;

public class DeleteColumnFormModel
{
    public DeleteColumnTasksDisposition TasksDisposition { get; set; } = DeleteColumnTasksDisposition.DeleteWithColumn;

    public Guid? TargetColumnId { get; set; }

    public ColumnTaskMovePlacement? MovePlacement { get; set; } = ColumnTaskMovePlacement.Start;
}
