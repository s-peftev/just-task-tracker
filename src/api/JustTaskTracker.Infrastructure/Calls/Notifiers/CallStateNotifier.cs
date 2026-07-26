using JustTaskTracker.Application.Calls.Notifiers;
using JustTaskTracker.Domain.Calls.Notifications;
using JustTaskTracker.Infrastructure.Boards.Hubs;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace JustTaskTracker.Infrastructure.Calls.Notifiers;

// AD-10: Calls' own notifier, not a reuse of Boards' IBoardActionNotifier -- both ultimately
// push through the same IHubContext<BoardActionsHub>, targeting the board's existing group.
public class CallStateNotifier(IHubContext<BoardActionsHub> hubContext) : ICallStateNotifier
{
    public Task NotifyAsync(CallStateNotification notification, CancellationToken ct) =>
        hubContext.Clients
            .Group(HubGroupNames.BoardActions.Get(notification.BoardId))
            .SendAsync(CallStateHubEvents.CallStateChanged, notification, ct);
}
