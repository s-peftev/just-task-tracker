using JustTaskTracker.Archival.Functions.Abstractions.Archiving;

namespace JustTaskTracker.Archival.Functions.Archiving.Summary;

public sealed class BoardExportSummaryWriterRegistry
{
    private readonly IReadOnlyDictionary<BoardExportSummaryFormat, IBoardExportSummaryWriter> _writers;

    public BoardExportSummaryWriterRegistry(IEnumerable<IBoardExportSummaryWriter> writers)
    {
        ArgumentNullException.ThrowIfNull(writers);

        _writers = writers.ToDictionary(w => w.Format);
    }

    public IBoardExportSummaryWriter Get(BoardExportSummaryFormat format) =>
        _writers.TryGetValue(format, out var writer)
            ? writer
            : throw new InvalidOperationException($"No summary writer registered for {format}.");
}
