namespace JustTaskTracker.Application.Assistant.Tools;

public interface IAssistantToolHandler
{
    string ToolName { get; }

    string Description { get; }

    Task<string> ExecuteAsync(Guid currentUserId, CancellationToken ct = default);
}
