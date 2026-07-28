using System.Text.Json.Serialization;
using JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;

namespace JustTaskTracker.WebUI.Domain.Calls.Notifications;

// Discriminator strings must match the server's JustTaskTracker.Domain.Calls.Notifications.CallStatePayload exactly.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ParticipantJoinedPayload), "participantJoined")]
[JsonDerivedType(typeof(ParticipantLeftPayload), "participantLeft")]
[JsonDerivedType(typeof(SessionClosedPayload), "sessionClosed")]
public abstract record CallStatePayload;
