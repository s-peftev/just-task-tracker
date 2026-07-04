namespace JustTaskTracker.Application.Boards.Jobs;

public interface IBoardExportRecoverySchedulerJob
{
    Task RunAsync(CancellationToken ct);
}
