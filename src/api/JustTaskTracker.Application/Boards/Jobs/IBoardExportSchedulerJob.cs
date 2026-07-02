namespace JustTaskTracker.Application.Boards.Jobs;

public interface IBoardExportSchedulerJob
{
    Task RunAsync(CancellationToken ct);
}
