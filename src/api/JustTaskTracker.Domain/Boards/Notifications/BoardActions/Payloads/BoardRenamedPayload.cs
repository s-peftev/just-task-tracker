namespace JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads;

public record BoardRenamedPayload(string Name) : BoardActionPayload;
