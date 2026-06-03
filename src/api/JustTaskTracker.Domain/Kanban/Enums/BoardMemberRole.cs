namespace JustTaskTracker.Domain.Kanban.Enums;

public enum BoardMemberRole : byte
{
    // Full access: profile, members, columns, tasks
    Owner = 1,

    // Manage members and columns/tasks; cannot delete board
    Admin = 2,

    // Create/edit tasks; view board
    User = 3,

    // Read-only
    Viewer = 4
}
