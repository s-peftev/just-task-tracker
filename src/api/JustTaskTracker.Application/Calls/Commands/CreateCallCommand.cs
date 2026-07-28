using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Calls.Constants;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Calls.Errors;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Calls.Commands;

public record CreateCallCommand(
    Guid BoardId,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    IReadOnlyList<Guid>? AllowedUserIds,
    IReadOnlyList<Guid>? LinkedTaskIds)
    : IRequest<Result<CallSessionDto>>, IRequireActiveBoard;

public class CreateCallCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IBoardRepository boardRepository,
    IBoardTaskRepository boardTaskRepository,
    ICallRepository callRepository,
    ICallSessionAllowedParticipantRepository allowedParticipantRepository,
    ICallSessionLinkedTaskRepository linkedTaskRepository,
    IAcsCallProvisioningService acsCallProvisioningService,
    ICallStateNotifier callStateNotifier,
    IProfilePhotoService profilePhotoService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateCallCommand, Result<CallSessionDto>>
{
    public async Task<Result<CallSessionDto>> Handle(CreateCallCommand request, CancellationToken ct)
    {
        var (board, userRole) = await boardRepository.GetBoardWithUserRoleAsync(request.BoardId, currentUserAccessor.AzureAdObjectId, ct);

        if (board is null)
            return Result<CallSessionDto>.Failure(GeneralErrors.NotFound);

        if (userRole is not { } role || !BoardRolePermissions.CanCreateCall(role))
            return Result<CallSessionDto>.Failure(GeneralErrors.Forbidden);

        var currentUser = await userRepository.GetUserByAzureAOIAsync(currentUserAccessor.AzureAdObjectId, ct);

        if (currentUser is null)
            return Result<CallSessionDto>.Failure(GeneralErrors.Unauthorized);

        IReadOnlyList<Guid> allowedUserIds = request.Visibility == CallVisibility.Restricted
            ? request.AllowedUserIds ?? []
            : [];

        // AD-4/AD-8: an allow-list entry only makes sense for someone who can actually be a call
        // participant at all -- reject up front rather than persisting a dangling/meaningless entry.
        foreach (var allowedUserId in allowedUserIds)
        {
            if (!await boardRepository.IsBoardMemberAsync(request.BoardId, allowedUserId, ct))
                return Result<CallSessionDto>.Failure(CallSessionsErrors.AllowedParticipantNotBoardMember);
        }

        // AD-13: linking is optional and not gated by Visibility; a task may already be linked to
        // other call sessions, so only de-dupe within this request, not across sessions.
        IReadOnlyList<Guid> linkedTaskIds = request.LinkedTaskIds is { Count: > 0 }
            ? request.LinkedTaskIds.Distinct().ToList()
            : [];

        foreach (var taskId in linkedTaskIds)
        {
            if (!await boardTaskRepository.ExistsInBoardAsync(taskId, request.BoardId, ct))
                return Result<CallSessionDto>.Failure(CallSessionsErrors.LinkedTaskNotOnBoard);
        }

        // AD-14: provision the ACS Room first; only persist once it exists.
        var acsRoomId = await acsCallProvisioningService.CreateRoomAsync(ct);

        var callSession = new CallSession
        {
            Id = Guid.NewGuid(),
            BoardId = request.BoardId,
            CreatedByUserId = currentUser.Id,
            Title = request.Title,
            Topic = request.Topic,
            Visibility = request.Visibility,
            AcsRoomId = acsRoomId,
            Status = CallStatus.Active,
            StartedAtUtc = dateTimeProvider.UtcNow
        };

        callRepository.Add(callSession);

        foreach (var allowedUserId in allowedUserIds)
        {
            allowedParticipantRepository.Add(new CallSessionAllowedParticipant
            {
                CallSessionId = callSession.Id,
                UserId = allowedUserId
            });
        }

        foreach (var taskId in linkedTaskIds)
        {
            linkedTaskRepository.Add(new CallSessionLinkedTask
            {
                CallSessionId = callSession.Id,
                TaskId = taskId
            });
        }

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            // AD-14: best-effort compensation -- don't leave an orphaned Room if the DB write failed.
            await acsCallProvisioningService.DeleteRoomAsync(acsRoomId, CancellationToken.None);
            throw;
        }

        await NotifyCallStartedAsync(request, callSession, board.Name, currentUser, allowedUserIds, ct);

        return Result<CallSessionDto>.Success(new CallSessionDto(
            callSession.Id,
            callSession.BoardId,
            callSession.CreatedByUserId,
            callSession.Title,
            callSession.Topic,
            callSession.Visibility,
            callSession.AcsRoomId,
            callSession.Status,
            callSession.StartedAtUtc,
            request.Visibility == CallVisibility.Restricted ? allowedUserIds : null,
            linkedTaskIds,
            callSession.CurrentPresenterUserId));
    }

    // AD-10: every board member if Open; only the allow-list plus Owner/Admin if Restricted.
    // The creator is always excluded -- they already know they just started this call.
    private async Task NotifyCallStartedAsync(
        CreateCallCommand request,
        CallSession callSession,
        string boardName,
        User currentUser,
        IReadOnlyList<Guid> allowedUserIds,
        CancellationToken ct)
    {
        var members = await boardRepository.GetMemberIdentitiesAsync(request.BoardId, ct);

        var eligibleMembers = request.Visibility == CallVisibility.Restricted
            ? members.Where(m => allowedUserIds.Contains(m.UserId) || BoardRolePermissions.CanBypassCallRestriction(m.Role))
            : members;

        var recipientAzureAdObjectIds = eligibleMembers
            .Where(m => m.UserId != currentUser.Id)
            .Select(m => m.AzureAdObjectId)
            .ToList();

        if (recipientAzureAdObjectIds.Count == 0)
            return;

        Func<UserReadModel, string?> profilePhotoUrlResolver = user =>
            user.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(user.Id, user.ProfilePhotoVersion);

        var createdBy = new UserReadModel(currentUser.Id, currentUser.Email, currentUser.DisplayName, currentUser.ProfilePhotoVersion)
            .ToDto(profilePhotoUrlResolver);

        var alert = new CallStartedAlert(callSession.BoardId, boardName, callSession.Id, callSession.Title, createdBy);

        await callStateNotifier.NotifyCallStartedAsync(alert, recipientAzureAdObjectIds, ct);
    }
}

public class CreateCallCommandValidator : AbstractValidator<CreateCallCommand>
{
    public CreateCallCommandValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("'Title' must not be empty.")
            .MaximumLength(CallFieldLengths.MaxTitleLength);

        RuleFor(x => x.Topic)
            .MaximumLength(CallFieldLengths.MaxTopicLength);

        RuleFor(x => x.Visibility)
            .IsInEnum();

        When(x => x.Visibility == CallVisibility.Restricted, () =>
        {
            RuleForEach(x => x.AllowedUserIds)
                .NotEmpty();
        });

        RuleForEach(x => x.LinkedTaskIds)
            .NotEmpty();
    }
}
