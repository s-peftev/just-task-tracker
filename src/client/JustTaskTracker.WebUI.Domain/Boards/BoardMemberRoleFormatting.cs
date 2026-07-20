using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards;

public static class BoardMemberRoleFormatting
{
    public static IReadOnlyList<BoardMemberRole> AssignableRoles { get; } =
    [
        BoardMemberRole.Admin,
        BoardMemberRole.ScrumMaster,
        BoardMemberRole.User,
    ];

    public const string GlobalAdminBoardRoleHint =
        "App admins can only be assigned the Board Admin role.";

    public static string Format(BoardMemberRole role) =>
        role switch
        {
            BoardMemberRole.Owner => "Owner",
            BoardMemberRole.Admin => "Admin",
            BoardMemberRole.ScrumMaster => "Scrum Master",
            BoardMemberRole.User => "User",
            _ => role.ToString(),
        };

    public static bool IsAssignableForGlobalAdmin(BoardMemberRole role) =>
        role == BoardMemberRole.Admin;
}
