namespace JustTaskTracker.WebUI.Domain.Auth;

public static class UserDisplayFormatting
{
    public static string GetShortName(string? displayName, string? email) =>
        string.IsNullOrWhiteSpace(displayName)
            ? email ?? string.Empty
            : displayName;

    public static string GetShortName(UserDto user) =>
        GetShortName(user.DisplayName, user.Email);

    public static string GetShortName(UserForBoardLookupDto user) =>
        GetShortName(user.DisplayName, user.Email);

    public static string GetShortName(UserWithRolesDto user) =>
        GetShortName(user.DisplayName, user.Email);

    public static string GetNameWithEmail(UserDto user)
    {
        if (string.IsNullOrWhiteSpace(user.DisplayName))
            return user.Email;

        return $"{user.DisplayName} ({user.Email})";
    }

    public static string AppendYouSuffix(string displayName) =>
        $"{displayName} (you)";

    public static string GetShortNameWithYouSuffix(UserDto user, bool isCurrentUser)
    {
        var name = GetShortName(user);
        return isCurrentUser ? AppendYouSuffix(name) : name;
    }
}
