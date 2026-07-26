using FluentValidation;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Calls.Commands.Internal;

// Invoked only from CallsWebhookController on Microsoft.Communication.CallParticipantRemoved (AD-12).
// Closes the session when this departure leaves zero active participants -- the sole authoritative
// auto-close path superseding Story 1.1's interim direct-write EndCallCommand.
public record RecordParticipantLeftCommand(string AcsRoomId, string AcsUserId, DateTime OccurredAtUtc) : IRequest<Result>;

public class RecordParticipantLeftCommandHandler(
    ICallRepository callRepository,
    ICallParticipantRepository callParticipantRepository,
    IAcsUserIdentityMappingRepository mappingRepository,
    ICallStateNotifier callStateNotifier,
    IUnitOfWork unitOfWork,
    ILogger<RecordParticipantLeftCommandHandler> logger)
    : IRequestHandler<RecordParticipantLeftCommand, Result>
{
    public async Task<Result> Handle(RecordParticipantLeftCommand request, CancellationToken ct)
    {
        var callSession = await callRepository.GetByAcsRoomIdAsync(request.AcsRoomId, ct);

        if (callSession is null)
        {
            logger.LogWarning("Ignoring CallParticipantRemoved for unknown ACS room {AcsRoomId}.", request.AcsRoomId);
            return Result.Success();
        }

        var mapping = await mappingRepository.GetByAcsCommunicationUserIdAsync(request.AcsUserId, ct);

        if (mapping is null)
        {
            logger.LogWarning("Ignoring CallParticipantRemoved for unmapped ACS user {AcsUserId}.", request.AcsUserId);
            return Result.Success();
        }

        // Fetched once and reasoned about in memory (rather than re-querying after mutating) so the
        // "who's left" count isn't skewed by an uncommitted change to the very row this method mutates.
        var activeParticipants = await callParticipantRepository.GetActiveParticipantsAsync(callSession.Id, ct);
        var participant = activeParticipants.FirstOrDefault(p => p.UserId == mapping.UserId);

        if (participant is null)
            return Result.Success(); // idempotent: already recorded as left, or never recorded as joined

        participant.LeftAtUtc = request.OccurredAtUtc;

        if (callSession.CurrentPresenterUserId == mapping.UserId)
            callSession.CurrentPresenterUserId = null; // AD-9: a departure releases the presenter lock same as an explicit stop-share

        var remainingActiveCount = activeParticipants.Count(p => p.Id != participant.Id);
        var sessionClosed = false;

        if (remainingActiveCount == 0 && callSession.Status == CallStatus.Active)
        {
            callSession.Status = CallStatus.Closed;
            callSession.EndedAtUtc = request.OccurredAtUtc;
            sessionClosed = true;
        }

        await unitOfWork.SaveChangesAsync(ct);

        await callStateNotifier.NotifyAsync(new CallStateNotification(
            callSession.BoardId,
            callSession.Id,
            CallStateNotificationType.ParticipantLeft,
            new ParticipantLeftPayload(mapping.UserId, request.OccurredAtUtc)), ct);

        if (sessionClosed)
        {
            await callStateNotifier.NotifyAsync(new CallStateNotification(
                callSession.BoardId,
                callSession.Id,
                CallStateNotificationType.SessionClosed,
                new SessionClosedPayload(request.OccurredAtUtc)), ct);
        }

        return Result.Success();
    }
}

public class RecordParticipantLeftCommandValidator : AbstractValidator<RecordParticipantLeftCommand>
{
    public RecordParticipantLeftCommandValidator()
    {
        RuleFor(x => x.AcsRoomId)
            .Must(roomId => !string.IsNullOrWhiteSpace(roomId))
            .WithMessage("'AcsRoomId' must not be empty.");

        RuleFor(x => x.AcsUserId)
            .Must(userId => !string.IsNullOrWhiteSpace(userId))
            .WithMessage("'AcsUserId' must not be empty.");
    }
}
