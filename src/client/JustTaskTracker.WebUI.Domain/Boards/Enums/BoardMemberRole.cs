namespace JustTaskTracker.WebUI.Domain.Boards.Enums;

public enum BoardMemberRole : byte
{
    // Full access: profile, members, columns, tasks
    Owner = 1,

    // Manage members and columns/tasks; cannot delete/rename board
    Admin = 2,

    // Manage columns/tasks; cannot delete/rename board
    ScrumMaster = 3,

    // Move and comment tasks only
    User = 4
}
