namespace JustTaskTracker.Application.Assistant.Tools;

internal class AssistantToolExecutor(IEnumerable<IAssistantToolHandler> handlers) : IAssistantToolExecutor
{
    private readonly IReadOnlyDictionary<string, IAssistantToolHandler> _handlers =
        handlers.ToDictionary(handler => handler.ToolName, StringComparer.Ordinal);

    public Task<string> ExecuteAsync(string toolName, Guid currentUserId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);

        if (!_handlers.TryGetValue(toolName, out var handler))
            return Task.FromResult(AssistantToolJson.Error($"Unknown tool '{toolName}'."));

        return handler.ExecuteAsync(currentUserId, ct);
    }
}
