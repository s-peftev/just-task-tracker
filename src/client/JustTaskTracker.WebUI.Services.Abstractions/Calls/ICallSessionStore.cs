using JustTaskTracker.WebUI.Domain.Calls;

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
    string? ErrorMessage { get; }
    Guid? CurrentCallId { get; }
    JoinCallResponse? CurrentJoinInfo { get; }

    event Action? StateChanged;

    Task OpenSidebarAsync(Guid boardId, CancellationToken ct = default);

    void CloseSidebar();

    Task<CallSessionDto?> CreateCallAsync(Guid boardId, string title, string? topic, CancellationToken ct = default);

    Task<JoinCallResponse?> JoinCallAsync(Guid callSessionId, CancellationToken ct = default);

    Task EndCurrentCallAsync(CancellationToken ct = default);

    void LeaveCurrentCall();
}
