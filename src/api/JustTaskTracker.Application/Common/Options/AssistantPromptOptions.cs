using JustTaskTracker.Application.Common.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class AssistantPromptOptions
{
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Returned when knowledge-base search succeeds but finds no relevant chunks,
    /// so the LLM is not called just to produce an "I don't know" reply.
    /// </summary>
    public string NoContextReply { get; set; } = string.Empty;

    /// <summary>
    /// Instruction appended after the current-user profile block so the model
    /// prioritizes this user's global role and entitlements over generic docs.
    /// </summary>
    public string UserContextInstruction { get; set; } = string.Empty;

    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// Optional. Omit for models that only allow the default temperature (e.g. gpt-5-mini).
    /// </summary>
    public float? Temperature { get; set; }

    public void Validate()
    {
        var section = ConfigSections.AssistantPrompt;

        if (string.IsNullOrWhiteSpace(SystemPrompt))
            throw new InvalidOperationException($"{section}:{nameof(SystemPrompt)} is not configured.");

        if (string.IsNullOrWhiteSpace(NoContextReply))
            throw new InvalidOperationException($"{section}:{nameof(NoContextReply)} is not configured.");

        if (string.IsNullOrWhiteSpace(UserContextInstruction))
            throw new InvalidOperationException($"{section}:{nameof(UserContextInstruction)} is not configured.");

        if (MaxOutputTokens <= 0)
            throw new InvalidOperationException(
                $"{section}:{nameof(MaxOutputTokens)} must be greater than 0.");

        if (Temperature is < 0f or > 2f)
            throw new InvalidOperationException(
                $"{section}:{nameof(Temperature)} must be between 0 and 2.");
    }
}
