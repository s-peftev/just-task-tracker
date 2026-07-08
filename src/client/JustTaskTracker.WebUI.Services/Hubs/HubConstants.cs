namespace JustTaskTracker.WebUI.Services.Hubs;

internal static class HubPaths
{
    public const string BoardExportStatus = "/hubs/board-export";
    public const string BoardActions = "/hubs/board-actions";
}

internal static class BoardExportHubEvents
{
    public const string ExportStatusChanged = "BoardExportStatusChanged";
    public const string ReExportStatusChanged = "BoardReExportStatusChanged";
}

internal static class BoardActionsHubEvents
{
    public const string BoardChanged = "BoardChanged";
}
