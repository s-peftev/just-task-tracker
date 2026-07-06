namespace JustTaskTracker.WebUI.Services.Hubs;

internal static class HubPaths
{
    public const string BoardExportStatus = "/hubs/board-export";
}

internal static class BoardExportHubEvents
{
    public const string ExportStatusChanged = "BoardExportStatusChanged";
    public const string ReExportStatusChanged = "BoardReExportStatusChanged";
}
