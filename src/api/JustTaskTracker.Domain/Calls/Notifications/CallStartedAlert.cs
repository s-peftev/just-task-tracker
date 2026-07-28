using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Calls.Notifications;

// AD-10: pushed via Clients.User(...), not the board group -- deliberately separate from
// CallStateNotification/CallStatePayload, which are the group-scoped in-call events.
public record CallStartedAlert(
    Guid BoardId,
    string BoardName,
    Guid CallSessionId,
    string Title,
    UserDto CreatedBy);
