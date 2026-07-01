using JustTaskTracker.Archival.Functions.Abstractions;
using JustTaskTracker.Archival.Functions.Contracts.Messaging;

namespace JustTaskTracker.Archival.Functions.Processing;

public class BoardExportProcessor : IBoardExportProcessor
{
    public Task RunAsync(BoardExportMessage message, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
