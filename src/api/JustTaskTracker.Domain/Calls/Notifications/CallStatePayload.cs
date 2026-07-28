using System.Text.Json.Serialization;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;

namespace JustTaskTracker.Domain.Calls.Notifications;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ParticipantJoinedPayload), "participantJoined")]
[JsonDerivedType(typeof(ParticipantLeftPayload), "participantLeft")]
[JsonDerivedType(typeof(SessionClosedPayload), "sessionClosed")]
public abstract record CallStatePayload;
