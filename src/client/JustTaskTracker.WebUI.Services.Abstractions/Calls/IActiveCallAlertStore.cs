using JustTaskTracker.WebUI.Domain.Calls.Notifications;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

/// <summary>
/// Cross-page "call started" alert (AD-10) -- live-only, no persisted state. A component
/// (typically the app shell) subscribes to <see cref="CallStarted"/> to surface it, e.g. as a snackbar.
/// </summary>
public interface IActiveCallAlertStore
{
    event Action<CallStartedAlert>? CallStarted;

    void ApplyCallStartedAlert(CallStartedAlert alert);
}
