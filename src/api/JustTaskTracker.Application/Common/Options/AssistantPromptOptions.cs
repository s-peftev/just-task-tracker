using JustTaskTracker.Application.Common.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class AssistantPromptOptions
{
    public string SystemPrompt { get; set; } = string.Empty;

    public int MaxOutputTokens { get; set; }

    public float Temperature { get; set; }

    public void Validate()
    {
        var section = ConfigSections.AssistantPrompt;

        if (string.IsNullOrWhiteSpace(SystemPrompt))
            throw new InvalidOperationException($"{section}:{nameof(SystemPrompt)} is not configured.");

        if (MaxOutputTokens <= 0)
            throw new InvalidOperationException(
                $"{section}:{nameof(MaxOutputTokens)} must be greater than 0.");

        if (Temperature is < 0f or > 2f)
            throw new InvalidOperationException(
                $"{section}:{nameof(Temperature)} must be between 0 and 2.");
    }
}
