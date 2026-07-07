using JustTaskTracker.WebUI.Domain.Boards.Notifications.BoardActions;
using JustTaskTracker.WebUI.Services.Abstractions.Auth;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Hubs;
using JustTaskTracker.WebUI.Services.Boards.Actions;
using JustTaskTracker.WebUI.Services.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.WebUI.Services.Hubs;

internal sealed class BoardActionsHubService(
    IAccessTokenProvider tokenProvider,
    IOptions<ApiClientOptions> options,
    IBoardDetailsStore boardDetailsStore,
    IProfileStore profileStore,
    ILogger<BoardActionsHubService> logger)
    : IBoardActionsHubService, IAsyncDisposable
{
    private static readonly TimeSpan[] ReconnectDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
    ];

    private readonly Dictionary<Guid, int> _subscriptionRefCounts = [];
    private readonly SemaphoreSlim _hubGate = new(1, 1);
    private HubConnection? _connection;

    public async Task JoinBoardAsync(Guid boardId, CancellationToken ct = default)
    {
        if (boardId == Guid.Empty)
            return;

        var serverSubscribe = false;

        await _hubGate.WaitAsync(ct);
        try
        {
            _subscriptionRefCounts.TryGetValue(boardId, out var refCount);

            if (refCount == 0)
                serverSubscribe = true;

            _subscriptionRefCounts[boardId] = refCount + 1;
        }
        finally
        {
            _hubGate.Release();
        }

        if (!serverSubscribe)
            return;

        await EnsureConnectedAsync(ct);

        try
        {
            await _connection!.InvokeAsync("SubscribeAsync", boardId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await RollbackSubscriptionRefAsync(boardId, ct);

            logger.LogWarning(
                ex,
                "Board actions hub subscribe failed for board {BoardId}.",
                boardId);
        }
    }

    public async Task LeaveBoardAsync(Guid boardId, CancellationToken ct = default)
    {
        if (boardId == Guid.Empty)
            return;

        var serverUnsubscribe = false;

        await _hubGate.WaitAsync(ct);
        try
        {
            if (!_subscriptionRefCounts.TryGetValue(boardId, out var refCount))
            {
                logger.LogDebug(
                    "Board actions hub unsubscribe ignored for board {BoardId} because it is not held.",
                    boardId);

                return;
            }

            if (refCount <= 0)
            {
                _subscriptionRefCounts.Remove(boardId);

                logger.LogDebug(
                    "Board actions hub unsubscribe ignored for board {BoardId} because ref count was non-positive.",
                    boardId);

                return;
            }

            if (refCount == 1)
            {
                _subscriptionRefCounts.Remove(boardId);
                serverUnsubscribe = true;
            }
            else
            {
                _subscriptionRefCounts[boardId] = refCount - 1;
            }
        }
        finally
        {
            _hubGate.Release();
        }

        if (!serverUnsubscribe || _connection is null)
            return;

        try
        {
            await _connection.InvokeAsync("UnsubscribeAsync", boardId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Board actions hub unsubscribe failed for board {BoardId}.",
                boardId);
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _hubGate.WaitAsync(ct);
        try
        {
            _subscriptionRefCounts.Clear();

            if (_connection is { } connection)
            {
                _connection = null;
                await connection.DisposeAsync();
            }
        }
        finally
        {
            _hubGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _hubGate.Dispose();

        if (_connection is { } connection)
            await connection.DisposeAsync();
    }

    private async Task RollbackSubscriptionRefAsync(Guid boardId, CancellationToken ct)
    {
        await _hubGate.WaitAsync(ct);
        try
        {
            if (!_subscriptionRefCounts.TryGetValue(boardId, out var refCount))
                return;

            if (refCount <= 1)
                _subscriptionRefCounts.Remove(boardId);
            else
                _subscriptionRefCounts[boardId] = refCount - 1;
        }
        finally
        {
            _hubGate.Release();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_connection?.State == HubConnectionState.Connected)
            return;

        await _hubGate.WaitAsync(ct);
        try
        {
            if (_connection?.State == HubConnectionState.Connected)
                return;

            if (_connection is null)
            {
                _connection = BuildConnection();
                RegisterHandlers();
            }

            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync(ct);
        }
        finally
        {
            _hubGate.Release();
        }
    }

    private HubConnection BuildConnection()
    {
        var apiOptions = options.Value;
        var hubUrl = new Uri(new Uri(apiOptions.BaseUrl), HubPaths.BoardActions).ToString();

        return new HubConnectionBuilder()
            .WithUrl(hubUrl, connectionOptions =>
            {
                connectionOptions.AccessTokenProvider = async () =>
                {
                    var tokenRequest = new AccessTokenRequestOptions { Scopes = apiOptions.Scopes };
                    var result = await tokenProvider.RequestAccessToken(tokenRequest);
                    return result.TryGetToken(out var token) ? token.Value : null;
                };
            })
            .WithAutomaticReconnect(ReconnectDelays)
            .Build();
    }

    private void RegisterHandlers()
    {
        if (_connection is null)
            return;

        _connection.On<BoardActionNotificationWireDto>(
            BoardActionsHubEvents.BoardChanged,
            OnBoardChanged);

        _connection.Reconnected += OnReconnectedAsync;

        _connection.Closed += ex =>
        {
            if (ex is not null)
                logger.LogWarning(ex, "Board actions hub connection closed unexpectedly.");

            return Task.CompletedTask;
        };
    }

    private void OnBoardChanged(BoardActionNotificationWireDto wire)
    {
        BoardActionPayload payload;

        try
        {
            payload = BoardActionPayloadParser.Parse(wire.Type, wire.Payload);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to parse board action payload for type {Type}.",
                wire.Type);

            return;
        }

        var notification = new BoardActionNotification(
            wire.BoardId,
            wire.Type,
            wire.ActorUserId,
            wire.OccurredAtUtc,
            payload);

        var currentUserId = profileStore.Profile?.Id ?? Guid.Empty;
        boardDetailsStore.ApplyBoardActionNotification(notification, currentUserId);
    }

    private async Task OnReconnectedAsync(string? connectionId)
    {
        logger.LogInformation(
            "Board actions hub reconnected. ConnectionId={ConnectionId}",
            connectionId);

        List<Guid> activeBoardIds;

        await _hubGate.WaitAsync();
        try
        {
            activeBoardIds = _subscriptionRefCounts.Keys.ToList();
        }
        finally
        {
            _hubGate.Release();
        }

        foreach (var boardId in activeBoardIds)
        {
            try
            {
                await _connection!.InvokeAsync("SubscribeAsync", boardId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to re-subscribe board {BoardId} after board actions hub reconnect.",
                    boardId);
            }
        }
    }
}
