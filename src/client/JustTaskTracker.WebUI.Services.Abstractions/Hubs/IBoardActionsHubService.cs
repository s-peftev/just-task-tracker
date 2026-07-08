namespace JustTaskTracker.WebUI.Services.Abstractions.Hubs;

public interface IBoardActionsHubService
{
    Task JoinBoardAsync(Guid boardId, CancellationToken ct = default);

    Task LeaveBoardAsync(Guid boardId, CancellationToken ct = default);

    Task DisconnectAsync(CancellationToken ct = default);
}
