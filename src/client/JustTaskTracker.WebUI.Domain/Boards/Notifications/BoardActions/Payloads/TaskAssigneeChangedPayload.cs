using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;

namespace JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAssigneeChangedPayload(
    Guid BoardTaskId,
    UserDto? Assignee) : BoardActionPayload;
