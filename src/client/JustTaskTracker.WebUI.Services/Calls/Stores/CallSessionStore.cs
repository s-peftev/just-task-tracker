using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Calls.Enums;
using JustTaskTracker.WebUI.Domain.Calls.Notifications;
using JustTaskTracker.WebUI.Domain.Calls.Notifications.Payloads;
using JustTaskTracker.WebUI.Domain.Common.Pagination;
using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Calls.Stores;

internal sealed class CallSessionStore(ICallsApiService callsApiService) : ICallSessionStore
{
    public const int HistoryPageSize = 10;

    private readonly List<CallSessionDto> _activeCalls = [];
    private readonly List<CallSessionHistoryDto> _history = [];
    private Guid _boardId;
    private int _historyPage = 1;

    public bool IsSidebarOpen { get; private set; }
    public bool IsLoadingActiveCalls { get; private set; }
    public IReadOnlyList<CallSessionDto> ActiveCalls => _activeCalls;
    public bool IsLoadingHistory { get; private set; }
    public bool IsLoadingMoreHistory { get; private set; }
    public IReadOnlyList<CallSessionHistoryDto> History => _history;
    public PaginationMetadata HistoryPagination { get; private set; } = new();
    public bool HasMoreHistory => _history.Count < HistoryPagination.TotalCount;
    public string? ErrorMessage { get; private set; }
    public Guid? CurrentCallId { get; private set; }
    public JoinCallResponse? CurrentJoinInfo { get; private set; }
    public Guid CurrentBoardId => _boardId;

    public event Action? StateChanged;
    public event Action<Guid>? CallSessionClosed;
    public event Action<Guid>? CallParticipantsChanged;
    public event Action<Guid, Guid?>? CallPresenterChanged;

    public async Task EnsureActiveCallsLoadedAsync(Guid boardId, CancellationToken ct = default)
    {
        _boardId = boardId;
        IsLoadingActiveCalls = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var calls = await callsApiService.GetActiveCallsAsync(boardId, ct);
            _activeCalls.Clear();
            _activeCalls.AddRange(calls);
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
        }
        finally
        {
            IsLoadingActiveCalls = false;
            NotifyStateChanged();
        }
    }

    public async Task OpenSidebarAsync(Guid boardId, CancellationToken ct = default)
    {
        IsSidebarOpen = true;
        NotifyStateChanged();

        await EnsureActiveCallsLoadedAsync(boardId, ct);

        _historyPage = 1;
        IsLoadingHistory = true;
        NotifyStateChanged();

        try
        {
            var page = await callsApiService.GetHistoryAsync(boardId, _historyPage, HistoryPageSize, ct);
            _history.Clear();
            _history.AddRange(page.Items);
            HistoryPagination = page.Metadata;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
        }
        finally
        {
            IsLoadingHistory = false;
            NotifyStateChanged();
        }
    }

    public async Task LoadMoreHistoryAsync(CancellationToken ct = default)
    {
        if (!HasMoreHistory || IsLoadingMoreHistory || IsLoadingHistory)
            return;

        IsLoadingMoreHistory = true;
        NotifyStateChanged();

        try
        {
            var page = await callsApiService.GetHistoryAsync(_boardId, _historyPage + 1, HistoryPageSize, ct);
            _history.AddRange(page.Items);
            HistoryPagination = page.Metadata;
            _historyPage++;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
        }
        finally
        {
            IsLoadingMoreHistory = false;
            NotifyStateChanged();
        }
    }

    public void CloseSidebar()
    {
        IsSidebarOpen = false;
        NotifyStateChanged();
    }

    public async Task<CallSessionDto?> CreateCallAsync(
        Guid boardId,
        string title,
        string? topic,
        CallVisibility visibility,
        IReadOnlyList<Guid>? allowedUserIds,
        IReadOnlyList<Guid>? linkedTaskIds,
        CancellationToken ct = default)
    {
        ErrorMessage = null;

        try
        {
            var request = new CreateCallRequest(boardId, title, topic, visibility, allowedUserIds, linkedTaskIds);
            var session = await callsApiService.CreateCallAsync(request, ct);
            _activeCalls.Add(session);
            NotifyStateChanged();

            return session;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
            NotifyStateChanged();

            return null;
        }
    }

    public async Task<JoinCallResponse?> JoinCallAsync(Guid callSessionId, CancellationToken ct = default)
    {
        ErrorMessage = null;

        try
        {
            var joinInfo = await callsApiService.JoinCallAsync(callSessionId, ct);
            CurrentCallId = callSessionId;
            CurrentJoinInfo = joinInfo;
            NotifyStateChanged();

            return joinInfo;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
            NotifyStateChanged();

            return null;
        }
    }

    public async Task EndCurrentCallAsync(CancellationToken ct = default)
    {
        if (CurrentCallId is not { } callId)
            return;

        try
        {
            await callsApiService.EndCallAsync(callId, ct);
            _activeCalls.RemoveAll(c => c.Id == callId);
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
        }
        finally
        {
            CurrentCallId = null;
            CurrentJoinInfo = null;
            NotifyStateChanged();
        }
    }

    public async Task<bool> RequestScreenShareAsync(CancellationToken ct = default)
    {
        if (CurrentCallId is not { } callId)
            return false;

        ErrorMessage = null;

        try
        {
            await callsApiService.RequestScreenShareAsync(callId, ct);

            return true;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
            NotifyStateChanged();

            return false;
        }
    }

    public async Task<bool> StopScreenShareAsync(CancellationToken ct = default)
    {
        if (CurrentCallId is not { } callId)
            return false;

        ErrorMessage = null;

        try
        {
            await callsApiService.StopScreenShareAsync(callId, ct);

            return true;
        }
        catch (ApiServiceException ex)
        {
            ErrorMessage = ex.Error?.Details is { Count: > 0 } details ? string.Join(" ", details) : ex.Message;
            NotifyStateChanged();

            return false;
        }
    }

    public void LeaveCurrentCall()
    {
        CurrentCallId = null;
        CurrentJoinInfo = null;
        NotifyStateChanged();
    }

    public async Task ApplyCallStateNotification(CallStateNotification notification)
    {
        switch (notification.Type)
        {
            case CallStateNotificationType.SessionClosed:
                _activeCalls.RemoveAll(c => c.Id == notification.CallSessionId);

                if (CurrentCallId == notification.CallSessionId)
                {
                    CurrentCallId = null;
                    CurrentJoinInfo = null;
                }

                NotifyStateChanged();
                CallSessionClosed?.Invoke(notification.CallSessionId);

                if (notification.BoardId == _boardId)
                    await RefreshHistoryAsync(notification.BoardId);

                break;

            case CallStateNotificationType.ParticipantJoined:
            case CallStateNotificationType.ParticipantLeft:
                // CallSessionDto now carries its own Participants -- a fresh, cheap batched fetch
                // (Story 3.2, AD-2/AD-10) beats trying to patch just this one field locally.
                await EnsureActiveCallsLoadedAsync(notification.BoardId);
                CallParticipantsChanged?.Invoke(notification.CallSessionId);
                break;

            case CallStateNotificationType.PresenterChanged:
                if (notification.Payload is PresenterChangedPayload presenterChanged)
                {
                    await EnsureActiveCallsLoadedAsync(notification.BoardId);
                    CallPresenterChanged?.Invoke(notification.CallSessionId, presenterChanged.PresenterUserId);
                }

                break;
        }
    }

    /// <summary>
    /// Re-fetches everything currently loaded into <see cref="History"/> (at least one page's worth)
    /// as page 1 of that same size, so a session that just closed (Story 1.2/AD-12) shows up
    /// immediately without discarding any "Show more" pages the user already loaded, and without
    /// needing a full page reload (Story 3.2, AC1).
    /// </summary>
    private async Task RefreshHistoryAsync(Guid boardId)
    {
        var loadedCount = Math.Max(_history.Count, HistoryPageSize);

        try
        {
            var page = await callsApiService.GetHistoryAsync(boardId, 1, loadedCount);
            _history.Clear();
            _history.AddRange(page.Items);
            HistoryPagination = page.Metadata;
            _historyPage = loadedCount / HistoryPageSize;
            NotifyStateChanged();
        }
        catch (ApiServiceException)
        {
            // Best-effort: this is a background refresh triggered by a live notification, not a
            // user action -- history will still self-correct next time the sidebar is (re)opened.
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
