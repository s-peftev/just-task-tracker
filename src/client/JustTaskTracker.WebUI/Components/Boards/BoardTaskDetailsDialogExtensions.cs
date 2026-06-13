using MudBlazor;

namespace JustTaskTracker.WebUI.Components.Boards;

public static class BoardTaskDetailsDialogExtensions
{
    public static Task<IDialogReference> ShowBoardTaskDetailsAsync(
        this IDialogService dialogService,
        Guid boardId,
        Guid columnId,
        Guid taskId,
        string? title = null)
    {
        var parameters = new DialogParameters<BoardTaskDetailsDialog>
        {
            { x => x.BoardId, boardId },
            { x => x.ColumnId, columnId },
            { x => x.TaskId, taskId },
        };

        var dialogTitle = string.IsNullOrWhiteSpace(title) ? "Task" : title;

        return dialogService.ShowAsync<BoardTaskDetailsDialog>(
            dialogTitle,
            parameters,
            BoardTaskDetailsDialog.DefaultOptions);
    }
}
