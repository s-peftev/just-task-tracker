using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using JustTaskTracker.Application.Assistant.Abstractions;
using JustTaskTracker.Infrastructure.Common.Options;

namespace JustTaskTracker.Infrastructure.Assistant;

internal class AzureAiSearchKnowledgeService(SearchClient searchClient, AiSearchOptions options) : IKnowledgeBaseSearchService
{
    public async Task<IReadOnlyList<RetrievedChunk>> SearchAsync(string queryText, CancellationToken ct = default)
    {
        var searchOptions = new SearchOptions
        {
            Size = options.TopK,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = options.SemanticConfigurationName
            },
            VectorSearch = new VectorSearchOptions()
        };

        var vectorQuery = new VectorizableTextQuery(queryText)
        {
            KNearestNeighborsCount = options.TopK
        };
        vectorQuery.Fields.Add(options.VectorFieldName);
        searchOptions.VectorSearch.Queries.Add(vectorQuery);
        searchOptions.Select.Add(options.ContentFieldName);

        var response = await searchClient.SearchAsync<SearchDocument>(queryText, searchOptions, ct);

        var chunks = new List<RetrievedChunk>();

        await foreach (var result in response.Value.GetResultsAsync())
        {
            if (!result.Document.TryGetValue(options.ContentFieldName, out var rawContent))
                continue;

            var content = rawContent switch
            {
                string text => text,
                null => null,
                _ => rawContent.ToString()
            };

            if (string.IsNullOrWhiteSpace(content))
                continue;

            chunks.Add(new RetrievedChunk(content));
        }

        return chunks;
    }
}
