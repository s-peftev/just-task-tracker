namespace JustTaskTracker.Application.Assistant.Tools;

internal class AssistantToolExecutor(IEnumerable<IAssistantToolHandler> handlers) : IAssistantToolExecutor
{
    private readonly IReadOnlyDictionary<string, IAssistantToolHandler> _handlers =
        handlers.ToDictionary(handler => handler.ToolName, StringComparer.Ordinal);

    public Task<string> ExecuteAsync(string toolName, Guid currentUserId, BinaryData arguments, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(arguments);

        if (!_handlers.TryGetValue(toolName, out var handler))
            return Task.FromResult(AssistantToolJson.Error($"Unknown tool '{toolName}'."));

        return handler.ExecuteAsync(currentUserId, arguments, ct);
    }
}
