using System.Text;
using System.Text.Json.Serialization;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using JustTaskTracker.API.Filters;
using JustTaskTracker.Application.Calls.Commands.Internal;
using JustTaskTracker.Domain.Calls.Constants;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Infrastructure.Common.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

// AD-11: no ASP.NET auth policy applies here (Event Grid can't carry it) -- instead, every
// request (handshake included) must carry the same shared-secret query-string parameter the
// Event Grid subscription's endpoint URL was configured with. That URL currently points at a
// local tunnel and will point at the cloud endpoint after redeploying the subscription; this
// controller only ever reads the incoming request, so it behaves identically either way.
[Route("calls")]
[ApiController]
[AllowAnonymous]
[SkipApiResponseEnvelope]
public class CallsWebhookController(ISender sender, AcsOptions acsOptions) : ControllerBase
{
    private const string ValidationKeyQueryParam = "validationKey";

    [HttpPost("acs-events")]
    public async Task<IActionResult> HandleAcsEvents(CancellationToken ct)
    {
        if (!Request.Query.TryGetValue(ValidationKeyQueryParam, out var providedKey)
            || !string.Equals(providedKey, acsOptions.WebhookValidationKey, StringComparison.Ordinal))
            return Unauthorized();

        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(ct);

        var events = EventGridEvent.ParseMany(BinaryData.FromString(payload));

        var anyFailure = false;

        foreach (var @event in events)
        {
            if (@event.TryGetSystemEventData(out var systemEventData)
                && systemEventData is SubscriptionValidationEventData validationEventData)
            {
                return Ok(new SubscriptionValidationResponse
                {
                    ValidationResponse = validationEventData.ValidationCode,
                });
            }

            var result = await DispatchAsync(@event, ct);

            if (!result.IsSuccess)
                anyFailure = true;
        }

        return anyFailure ? BadRequest() : Ok();
    }

    // AD-12: only these three carry call lifecycle state; anything else is acknowledged and ignored.
    private Task<Result> DispatchAsync(EventGridEvent @event, CancellationToken ct) => @event.EventType switch
    {
        AcsEventGridEventTypes.CallParticipantAdded => DispatchParticipantAdded(@event, ct),
        AcsEventGridEventTypes.CallParticipantRemoved => DispatchParticipantRemoved(@event, ct),
        AcsEventGridEventTypes.CallEnded => DispatchCallEnded(@event, ct),
        _ => Task.FromResult(Result.Success())
    };

    private Task<Result> DispatchParticipantAdded(EventGridEvent @event, CancellationToken ct)
    {
        var data = @event.Data!.ToObjectFromJson<AcsCallParticipantEventData>()!;

        return sender.Send(
            new RecordParticipantJoinedCommand(data.Room.Id, data.User.CommunicationIdentifier.RawId, @event.EventTime.UtcDateTime),
            ct);
    }

    private Task<Result> DispatchParticipantRemoved(EventGridEvent @event, CancellationToken ct)
    {
        var data = @event.Data!.ToObjectFromJson<AcsCallParticipantEventData>()!;

        return sender.Send(
            new RecordParticipantLeftCommand(data.Room.Id, data.User.CommunicationIdentifier.RawId, @event.EventTime.UtcDateTime),
            ct);
    }

    private Task<Result> DispatchCallEnded(EventGridEvent @event, CancellationToken ct)
    {
        var data = @event.Data!.ToObjectFromJson<AcsCallEndedEventData>()!;

        return sender.Send(new RecordCallEndedCommand(data.Room.Id, @event.EventTime.UtcDateTime), ct);
    }

    // Minimal shapes of the documented payloads (learn.microsoft.com/azure/event-grid/communication-services-voice-video-events)
    // -- only the fields this controller actually needs to correlate events, not the full schema.
    private sealed record AcsRoomRef([property: JsonPropertyName("id")] string Id);

    private sealed record AcsCommunicationIdentifierRef([property: JsonPropertyName("rawId")] string RawId);

    private sealed record AcsParticipantEventUserRef(
        [property: JsonPropertyName("communicationIdentifier")] AcsCommunicationIdentifierRef CommunicationIdentifier);

    private sealed record AcsCallParticipantEventData(
        [property: JsonPropertyName("room")] AcsRoomRef Room,
        [property: JsonPropertyName("user")] AcsParticipantEventUserRef User);

    private sealed record AcsCallEndedEventData([property: JsonPropertyName("room")] AcsRoomRef Room);
}
