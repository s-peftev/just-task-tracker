using JustTaskTracker.Archival.Functions.Contracts.Messaging;

namespace JustTaskTracker.Archival.Functions.Abstractions.Processing;

public interface IBoardExportProcessor
{
    Task RunAsync(BoardExportMessage message, CancellationToken ct = default);
}
