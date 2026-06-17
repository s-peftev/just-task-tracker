using JustTaskTracker.WebUI.Domain.Boards.Enums;

namespace JustTaskTracker.WebUI.Domain.Boards.Authorization;

public static class BoardRolePermissions
{
    public static bool CanViewBoard(BoardMemberRole role) =>
        role is BoardMemberRole.Owner
            or BoardMemberRole.Admin
            or BoardMemberRole.ScrumMaster
            or BoardMemberRole.User;

    public static bool CanRenameBoard(BoardMemberRole role) =>
        role is BoardMemberRole.Owner;

    public static bool CanDeleteBoard(BoardMemberRole role) =>
        role is BoardMemberRole.Owner;

    public static bool CanManageMembers(BoardMemberRole role) =>
        role is BoardMemberRole.Owner or BoardMemberRole.Admin;

    public static bool CanManageColumns(BoardMemberRole role) =>
        role is BoardMemberRole.Owner or BoardMemberRole.Admin or BoardMemberRole.ScrumMaster;

    public static bool CanManageTasks(BoardMemberRole role) =>
        role is BoardMemberRole.Owner or BoardMemberRole.Admin or BoardMemberRole.ScrumMaster;

    public static bool CanMoveTasks(BoardMemberRole role) =>
        role is BoardMemberRole.Owner
            or BoardMemberRole.Admin
            or BoardMemberRole.ScrumMaster
            or BoardMemberRole.User;

    public static bool CanCommentOnTasks(BoardMemberRole role) =>
        role is BoardMemberRole.Owner
            or BoardMemberRole.Admin
            or BoardMemberRole.ScrumMaster
            or BoardMemberRole.User;

    public static bool CanDownloadAttachments(BoardMemberRole role) =>
        role is BoardMemberRole.Owner
            or BoardMemberRole.Admin
            or BoardMemberRole.ScrumMaster
            or BoardMemberRole.User;

    public static bool CanLeaveBoard(BoardMemberRole role) =>
        role is not BoardMemberRole.Owner;
}
