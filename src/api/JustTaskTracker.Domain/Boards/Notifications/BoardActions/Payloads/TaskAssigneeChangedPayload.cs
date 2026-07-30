using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record TaskAssigneeChangedPayload(
    Guid BoardTaskId,
    UserDto? Assignee) : BoardActionPayload;
