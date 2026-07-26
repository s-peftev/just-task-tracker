using FluentValidation;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Calls.Commands.Internal;

// Invoked only from CallsWebhookController on Microsoft.Communication.CallParticipantAdded (AD-12).
// AcsRoomId/AcsUserId/OccurredAtUtc are sourced straight from the event payload -- never resolved
// against "now" or a user's own request, since this is a server-to-server, unauthenticated trigger.
public record RecordParticipantJoinedCommand(string AcsRoomId, string AcsUserId, DateTime OccurredAtUtc) : IRequest<Result>;

public class RecordParticipantJoinedCommandHandler(
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    IAcsUserIdentityMappingRepository mappingRepository,
    ICallStateNotifier callStateNotifier,
    IUnitOfWork unitOfWork,
    ILogger<RecordParticipantJoinedCommandHandler> logger)
    : IRequestHandler<RecordParticipantJoinedCommand, Result>
{
    public async Task<Result> Handle(RecordParticipantJoinedCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByAcsRoomIdAsync(request.AcsRoomId, ct);

        if (callSession is null)
        {
            logger.LogWarning("Ignoring CallParticipantAdded for unknown ACS room {AcsRoomId}.", request.AcsRoomId);
            return Result.Success();
        }

        var mapping = await mappingRepository.GetByAcsCommunicationUserIdAsync(request.AcsUserId, ct);

        if (mapping is null)
        {
            logger.LogWarning("Ignoring CallParticipantAdded for unmapped ACS user {AcsUserId}.", request.AcsUserId);
            return Result.Success();
        }

        var existingActiveParticipant = await callParticipantRepository.GetActiveParticipantAsync(callSession.Id, mapping.UserId, ct);

        if (existingActiveParticipant is not null)
            return Result.Success(); // idempotent: duplicate delivery of the same join

        callParticipantRepository.Add(new CallParticipant
        {
            Id = Guid.NewGuid(),
            CallSessionId = callSession.Id,
            UserId = mapping.UserId,
            JoinedAtUtc = request.OccurredAtUtc
        });

        // A concurrently-processed duplicate delivery could still race this check; the filtered
        // unique index (see the CallParticipants migration) is the actual idempotency guarantee --
        // a losing race surfaces as a failed request that Event Grid retries, and the retry then
        // finds the row the other request already inserted.
        await unitOfWork.SaveChangesAsync(ct);

        await callStateNotifier.NotifyAsync(new CallStateNotification(
            callSession.BoardId,
            callSession.Id,
            CallStateNotificationType.ParticipantJoined,
            new ParticipantJoinedPayload(mapping.UserId, request.OccurredAtUtc)), ct);

        return Result.Success();
    }
}

public class RecordParticipantJoinedCommandValidator : AbstractValidator<RecordParticipantJoinedCommand>
{
    public RecordParticipantJoinedCommandValidator()
    {
        RuleFor(x => x.AcsRoomId)
            .Must(roomId => !string.IsNullOrWhiteSpace(roomId))
            .WithMessage("'AcsRoomId' must not be empty.");

        RuleFor(x => x.AcsUserId)
            .Must(userId => !string.IsNullOrWhiteSpace(userId))
            .WithMessage("'AcsUserId' must not be empty.");
    }
}
