using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Calls.Enums;
using JustTaskTracker.WebUI.Domain.Calls.Notifications;
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

    public event Action? StateChanged;
    public event Action<Guid>? CallSessionClosed;
    public event Action<Guid>? CallParticipantsChanged;

    public async Task OpenSidebarAsync(Guid boardId, CancellationToken ct = default)
    {
        _boardId = boardId;
        IsSidebarOpen = true;
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

    public void LeaveCurrentCall()
    {
        CurrentCallId = null;
        CurrentJoinInfo = null;
        NotifyStateChanged();
    }

    public void ApplyCallStateNotification(CallStateNotification notification)
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
                break;

            case CallStateNotificationType.ParticipantJoined:
            case CallStateNotificationType.ParticipantLeft:
                CallParticipantsChanged?.Invoke(notification.CallSessionId);
                break;
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
