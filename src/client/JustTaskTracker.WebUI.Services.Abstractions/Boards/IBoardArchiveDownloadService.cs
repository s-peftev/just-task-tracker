namespace JustTaskTracker.WebUI.Services.Abstractions.Boards;

public interface IBoardArchiveDownloadService
{
    Task DownloadAsync(Guid boardId, CancellationToken ct = default);
}
