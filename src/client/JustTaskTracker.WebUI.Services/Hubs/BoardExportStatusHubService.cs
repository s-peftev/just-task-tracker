using JustTaskTracker.WebUI.Domain.Boards.Messaging;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Abstractions.Hubs;
using JustTaskTracker.WebUI.Services.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.WebUI.Services.Hubs;

internal sealed class BoardExportStatusHubService(
    IAccessTokenProvider tokenProvider,
    IOptions<ApiClientOptions> options,
    IBoardStore boardStore,
    IBoardDetailsStore boardDetailsStore,
    ILogger<BoardExportStatusHubService> logger)
    : IBoardExportStatusHubService, IAsyncDisposable
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

    public async Task SubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default)
    {
        var ids = NormalizeBoardIds(boardIds);
        if (ids.Count == 0)
            return;

        List<Guid> serverSubscribeIds;

        await _hubGate.WaitAsync(ct);
        try
        {
            serverSubscribeIds = AcquireSubscriptionRefs(ids);
        }
        finally
        {
            _hubGate.Release();
        }

        if (serverSubscribeIds.Count == 0)
            return;

        await EnsureConnectedAsync(ct);

        try
        {
            await _connection!.InvokeAsync("SubscribeAsync", (object)serverSubscribeIds, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await RollbackSubscriptionRefsAsync(ids, ct);

            logger.LogWarning(
                ex,
                "Board export hub subscribe failed for {Count} board(s).",
                serverSubscribeIds.Count);
        }
    }

    public async Task UnsubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default)
    {
        var ids = NormalizeBoardIds(boardIds);
        if (ids.Count == 0)
            return;

        List<Guid> serverUnsubscribeIds;

        await _hubGate.WaitAsync(ct);
        try
        {
            serverUnsubscribeIds = ReleaseSubscriptionRefs(ids);
        }
        finally
        {
            _hubGate.Release();
        }

        if (serverUnsubscribeIds.Count == 0 || _connection is null)
            return;

        try
        {
            await _connection.InvokeAsync("UnsubscribeAsync", (object)serverUnsubscribeIds, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Board export hub unsubscribe failed for {Count} board(s).",
                serverUnsubscribeIds.Count);
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

    private List<Guid> AcquireSubscriptionRefs(IReadOnlyList<Guid> ids)
    {
        var serverSubscribeIds = new List<Guid>(ids.Count);

        foreach (var id in ids)
        {
            _subscriptionRefCounts.TryGetValue(id, out var refCount);

            if (refCount == 0)
                serverSubscribeIds.Add(id);

            _subscriptionRefCounts[id] = refCount + 1;
        }

        return serverSubscribeIds;
    }

    private List<Guid> ReleaseSubscriptionRefs(IReadOnlyList<Guid> ids)
    {
        var serverUnsubscribeIds = new List<Guid>(ids.Count);

        foreach (var id in ids)
        {
            if (!_subscriptionRefCounts.TryGetValue(id, out var refCount))
            {
                logger.LogDebug(
                    "Board export hub unsubscribe ignored for board {BoardId} because it is not held.",
                    id);

                continue;
            }

            if (refCount <= 0)
            {
                _subscriptionRefCounts.Remove(id);

                logger.LogDebug(
                    "Board export hub unsubscribe ignored for board {BoardId} because ref count was non-positive.",
                    id);

                continue;
            }

            if (refCount == 1)
            {
                _subscriptionRefCounts.Remove(id);
                serverUnsubscribeIds.Add(id);
            }
            else
            {
                _subscriptionRefCounts[id] = refCount - 1;
            }
        }

        return serverUnsubscribeIds;
    }

    private async Task RollbackSubscriptionRefsAsync(IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        await _hubGate.WaitAsync(ct);
        try
        {
            foreach (var id in ids)
            {
                if (!_subscriptionRefCounts.TryGetValue(id, out var refCount))
                    continue;

                if (refCount <= 1)
                    _subscriptionRefCounts.Remove(id);
                else
                    _subscriptionRefCounts[id] = refCount - 1;
            }
        }
        finally
        {
            _hubGate.Release();
        }
    }

    private static List<Guid> NormalizeBoardIds(IReadOnlyList<Guid> boardIds) =>
        boardIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

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
        var hubUrl = new Uri(new Uri(apiOptions.BaseUrl), HubPaths.BoardExportStatus).ToString();

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

        _connection.On<BoardExportStatusChangedNotification>(
            BoardExportHubEvents.ExportStatusChanged,
            OnExportStatusChanged);

        _connection.On<BoardExportStatusChangedNotification>(
            BoardExportHubEvents.ReExportStatusChanged,
            OnReExportStatusChanged);

        _connection.Reconnected += OnReconnectedAsync;

        _connection.Closed += ex =>
        {
            if (ex is not null)
                logger.LogWarning(ex, "Board export hub connection closed unexpectedly.");

            return Task.CompletedTask;
        };
    }

    private void OnExportStatusChanged(BoardExportStatusChangedNotification notification)
    {
        boardStore.ApplyExportStatusChanged(notification.BoardId, notification.Status);
        boardDetailsStore.ApplyExportStatusChanged(notification.BoardId, notification.Status);
    }

    private void OnReExportStatusChanged(BoardExportStatusChangedNotification notification)
    {
        boardStore.ApplyReExportStatusChanged(notification.BoardId, notification.Status);
        boardDetailsStore.ApplyReExportStatusChanged(
            notification.BoardId,
            notification.Status,
            notification.ExportOptions);
    }

    private async Task OnReconnectedAsync(string? connectionId)
    {
        logger.LogInformation(
            "Board export hub reconnected. ConnectionId={ConnectionId}",
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

        if (activeBoardIds.Count == 0)
            return;

        try
        {
            await _connection!.InvokeAsync("SubscribeAsync", (object)activeBoardIds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to re-subscribe after board export hub reconnect.");
        }
    }
}
