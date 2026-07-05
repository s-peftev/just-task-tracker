namespace JustTaskTracker.Infrastructure.Common.Constants.Hubs;

public static class HubGroupNames
{
    public static class BoardExportStatus
    {
        public static string Get(Guid boardId) => $"board-export:{boardId:N}";
    }
}