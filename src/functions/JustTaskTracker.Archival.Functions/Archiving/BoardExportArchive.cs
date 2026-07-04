namespace JustTaskTracker.Archival.Functions.Archiving;

/// <summary>
/// Finished board export package ready for blob upload. Caller is responsible for disposal.
/// </summary>
public sealed class BoardExportArchive(Stream content, string fileName) : IAsyncDisposable
{
    public Stream Content { get; } = content;

    /// <summary>Suggested blob file name, e.g. "{boardId:D}.zip".</summary>
    public string FileName { get; } = fileName;

    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
    }
}
