namespace JustTaskTracker.WebUI.Services.Abstractions.Hubs;

public interface IBoardExportStatusHubService
{
    Task SubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default);

    Task UnsubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default);

    Task DisconnectAsync(CancellationToken ct = default);
}
