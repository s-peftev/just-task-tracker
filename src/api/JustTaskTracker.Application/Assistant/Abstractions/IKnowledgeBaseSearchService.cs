namespace JustTaskTracker.Application.Assistant.Abstractions;

public interface IKnowledgeBaseSearchService
{
    Task<IReadOnlyList<RetrievedChunk>> SearchAsync(string queryText, CancellationToken ct = default);
}
