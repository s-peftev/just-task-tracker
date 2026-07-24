using JustTaskTracker.WebUI.Domain.Calls;
using JustTaskTracker.WebUI.Domain.Calls.Enums;
using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Calls.Stores;

internal sealed class CallSessionStore(ICallsApiService callsApiService) : ICallSessionStore
{
    private readonly List<CallSessionDto> _activeCalls = [];

    public bool IsSidebarOpen { get; private set; }
    public bool IsLoadingActiveCalls { get; private set; }
    public IReadOnlyList<CallSessionDto> ActiveCalls => _activeCalls;
    public string? ErrorMessage { get; private set; }
    public Guid? CurrentCallId { get; private set; }
    public JoinCallResponse? CurrentJoinInfo { get; private set; }

    public event Action? StateChanged;

    public async Task OpenSidebarAsync(Guid boardId, CancellationToken ct = default)
    {
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
    }

    public void CloseSidebar()
    {
        IsSidebarOpen = false;
        NotifyStateChanged();
    }

    public async Task<CallSessionDto?> CreateCallAsync(Guid boardId, string title, string? topic, CancellationToken ct = default)
    {
        ErrorMessage = null;

        try
        {
            var request = new CreateCallRequest(boardId, title, topic, CallVisibility.Open);
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

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
