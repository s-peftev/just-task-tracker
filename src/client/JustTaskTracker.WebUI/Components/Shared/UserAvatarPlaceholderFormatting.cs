using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Components.Shared;

public static class UserAvatarPlaceholderFormatting
{
    public static string GetDisplaySource(string? displayName, string? email) =>
        UserDisplayFormatting.GetShortName(displayName, email);

    public static char GetLetter(string? displayName, string? email) =>
        char.ToUpperInvariant(
            GetDisplaySource(displayName, email) is { Length: > 0 } source
                ? source[0]
                : '?');
}
