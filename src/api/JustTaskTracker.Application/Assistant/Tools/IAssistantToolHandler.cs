namespace JustTaskTracker.Application.Assistant.Tools;

public interface IAssistantToolHandler
{
    string ToolName { get; }

    string Description { get; }

    /// <summary>
    /// JSON Schema for the tool's function parameters (OpenAI <c>parameters</c>).
    /// Use an empty object schema when the tool has no parameters.
    /// </summary>
    BinaryData ParametersSchema { get; }

    /// <param name="currentUserId">Authenticated app user id — never taken from model arguments.</param>
    /// <param name="arguments">Raw JSON arguments from the model (<c>{}</c> when the tool has no parameters).</param>
    Task<string> ExecuteAsync(Guid currentUserId, BinaryData arguments, CancellationToken ct = default);
}
