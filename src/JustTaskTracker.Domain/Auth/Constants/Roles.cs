namespace JustTaskTracker.Domain.Auth.Constants;

/// <summary>
/// Stable roles aligned with <c>auth.Roles</c> seed data.
/// </summary>
public static class Roles
{
    public static readonly RoleDefinition Admin = new(1, "ADMIN", "Admin");
    public static readonly RoleDefinition User  = new(2, "USER",  "User");
    public static readonly IReadOnlyList<RoleDefinition> All = [Admin, User];
}
public record RoleDefinition(int Id, string NormalizedName, string Name);
