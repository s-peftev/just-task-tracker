namespace JustTaskTracker.Domain.Auth.Constants;

public static class Roles
{
    public const string Admin = "ADMIN";
    public const string User = "USER";
    public const string Guest = "GUEST";

    public static readonly IReadOnlyList<string> All = [Admin, User, Guest];
}
