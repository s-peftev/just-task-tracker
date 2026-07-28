using JustTaskTracker.Application.Users.ReadModels;

namespace JustTaskTracker.Application.Calls.ReadModels;

// One row per CallParticipant record of a closed session -- a user who rejoined the same session
// several times produces several rows here, collapsed into one CallHistoryParticipantDto (first
// join to last leave) by the caller after fetch.
public record CallParticipantEventReadModel(UserReadModel User, DateTime JoinedAtUtc, DateTime? LeftAtUtc);
