using System.Text.Json;

namespace JustTaskTracker.Application.Assistant.Tools;

internal static class AssistantToolJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(object value) =>
        JsonSerializer.Serialize(value, Options);

    public static string Error(string message) =>
        Serialize(new { error = message });
}
