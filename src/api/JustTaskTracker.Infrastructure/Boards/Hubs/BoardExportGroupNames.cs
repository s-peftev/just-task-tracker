namespace JustTaskTracker.Infrastructure.Boards.Hubs;

public static class BoardExportGroupNames
{
    public static string ForBoard(Guid boardId) => $"board-export:{boardId:N}";
}