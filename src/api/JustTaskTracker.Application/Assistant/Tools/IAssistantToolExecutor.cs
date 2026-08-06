namespace JustTaskTracker.Application.Assistant.Tools;

public interface IAssistantToolExecutor
{
    /// <summary>
    /// Executes a whitelisted assistant tool for <paramref name="currentUserId"/> and returns a JSON payload for the model.
    /// </summary>
    /// <param name="toolName">Tool name as provided by the model (must match <see cref="AssistantToolNames"/>).</param>
    /// <param name="currentUserId">Authenticated app user id — never taken from model arguments.</param>
    /// <param name="arguments">Raw JSON arguments from the model.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> ExecuteAsync(string toolName, Guid currentUserId, BinaryData arguments, CancellationToken ct = default);
}
