namespace JustTaskTracker.WebUI.Components.Boards;

internal static class BoardTaskRoutes
{
    public static string GetBoardUrl(Guid boardId) => $"/boards/{boardId}";

    public static string GetTaskUrl(Guid boardId, Guid columnId, Guid taskId) =>
        $"/boards/{boardId}/columns/{columnId}/tasks/{taskId}";

    public static bool HasTaskSegment(string uri) =>
        uri.Contains("/tasks/", StringComparison.OrdinalIgnoreCase);
}
