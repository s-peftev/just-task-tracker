namespace JustTaskTracker.WebUI.Domain.Common.Searching;

public record TextSearchOptions<T>(
    string? Search,
    IReadOnlyList<T>? SearchIn = null);
