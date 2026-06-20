namespace JustTaskTracker.WebUI.Components.Shared;

public static class UserAvatarPlaceholderFormatting
{
    public static string GetDisplaySource(string? displayName, string? email) =>
        string.IsNullOrWhiteSpace(displayName)
            ? email ?? string.Empty
            : displayName;

    public static char GetLetter(string? displayName, string? email) =>
        char.ToUpperInvariant(
            GetDisplaySource(displayName, email) is { Length: > 0 } source
                ? source[0]
                : '?');
}
