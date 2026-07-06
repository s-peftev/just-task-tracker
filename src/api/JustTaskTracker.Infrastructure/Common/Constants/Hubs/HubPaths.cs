namespace JustTaskTracker.Infrastructure.Common.Constants.Hubs;

public static class HubPaths
{
    public const string Root = "/hubs";

    public const string BoardExportStatus = $"{Root}/board-export";
    public const string BoardActivity = $"{Root}/board-activity";
    public const string BoardActions = $"{Root}/board-actions";
}
