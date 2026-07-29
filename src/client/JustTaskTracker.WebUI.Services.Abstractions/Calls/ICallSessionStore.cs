using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Calls.Enums;
using JustTaskTracker.WebUI.Domain.Calls.Notifications;
using JustTaskTracker.WebUI.Domain.Common.Pagination;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

/// <summary>
/// Scoped store for the Calls sidebar and the local user's current call session state.
/// Does not touch the ACS Calling SDK directly (that needs live ElementReferences owned
/// by a component) -- it owns API-level state only: sidebar visibility, the active-calls
/// list, and which call (if any) the local user has joined.
/// </summary>
public interface ICallSessionStore
{
    bool IsSidebarOpen { get; }
    bool IsLoadingActiveCalls { get; }
    IReadOnlyList<CallSessionDto> ActiveCalls { get; }
    bool IsLoadingHistory { get; }
    bool IsLoadingMoreHistory { get; }
    IReadOnlyList<CallSessionHistoryDto> History { get; }
    PaginationMetadata HistoryPagination { get; }
    bool HasMoreHistory { get; }
    string? ErrorMessage { get; }
    Guid? CurrentCallId { get; }
    JoinCallResponse? CurrentJoinInfo { get; }

    /// <summary>The board whose active calls are currently loaded (<see cref="Guid.Empty"/> if none yet).</summary>
    Guid CurrentBoardId { get; }

    event Action? StateChanged;

    /// <summary>Raised with the closed call's id when a <see cref="CallStateNotificationType.SessionClosed"/> notification arrives.</summary>
    event Action<Guid>? CallSessionClosed;

    /// <summary>Raised with the call's id when a participant joins or leaves it, so an open CallPage can refresh its roster.</summary>
    event Action<Guid>? CallParticipantsChanged;

    /// <summary>Raised with the call's id and the new presenter (null if the slot was freed) on a <see cref="CallStateNotificationType.PresenterChanged"/> notification.</summary>
    event Action<Guid, Guid?>? CallPresenterChanged;

    /// <summary>
    /// Loads (or reloads) a board's active calls only -- cheap enough (a handful of batched
    /// queries server-side) to call eagerly on board load for the header badge, and again on every
    /// relevant live-state notification, without waiting for the sidebar to be opened.
    /// </summary>
    Task EnsureActiveCallsLoadedAsync(Guid boardId, CancellationToken ct = default);

    Task OpenSidebarAsync(Guid boardId, CancellationToken ct = default);

    Task LoadMoreHistoryAsync(CancellationToken ct = default);

    void CloseSidebar();

    Task<CallSessionDto?> CreateCallAsync(
        Guid boardId,
        string title,
        string? topic,
        CallVisibility visibility,
        IReadOnlyList<Guid>? allowedUserIds,
        IReadOnlyList<Guid>? linkedTaskIds,
        CancellationToken ct = default);

    Task<JoinCallResponse?> JoinCallAsync(Guid callSessionId, CancellationToken ct = default);

    Task EndCurrentCallAsync(CancellationToken ct = default);

    /// <summary>
    /// Requests the presenter lock for the current call (AD-9). Returns <see langword="false"/>
    /// without throwing if someone else already holds it -- <see cref="ErrorMessage"/> carries the reason.
    /// </summary>
    Task<bool> RequestScreenShareAsync(CancellationToken ct = default);

    /// <summary>Releases the presenter lock for the current call, if the local user holds it.</summary>
    Task<bool> StopScreenShareAsync(CancellationToken ct = default);

    void LeaveCurrentCall();

    Task ApplyCallStateNotification(CallStateNotification notification);
}
