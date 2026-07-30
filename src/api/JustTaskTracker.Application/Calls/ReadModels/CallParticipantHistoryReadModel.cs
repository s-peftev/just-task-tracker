namespace JustTaskTracker.Application.Calls.ReadModels;

// One row per distinct participant of a closed CallSession -- a user who rejoined the same
// session several times is collapsed into a single entry spanning their first join to last leave.
public record CallParticipantHistoryReadModel(
    Guid UserId,
    DateTime FirstJoinedAtUtc,
    DateTime? LastLeftAtUtc);
