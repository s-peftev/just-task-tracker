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

    private readonly HashSet<Guid> _subscribedBoardIds = [];
    private readonly SemaphoreSlim _connectGate = new(1, 1);
    private HubConnection? _connection;

    public async Task SubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default)
    {
        var newIds = boardIds
            .Where(id => id != Guid.Empty && !_subscribedBoardIds.Contains(id))
            .Distinct()
            .ToList();

        if (newIds.Count == 0)
            return;

        await EnsureConnectedAsync(ct);

        try
        {
            await _connection!.InvokeAsync("SubscribeAsync", (object)newIds, ct);

            foreach (var id in newIds)
                _subscribedBoardIds.Add(id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Board export hub subscribe failed for {Count} board(s).", newIds.Count);
        }
    }

    public async Task UnsubscribeAsync(IReadOnlyList<Guid> boardIds, CancellationToken ct = default)
    {
        var toRemove = boardIds
            .Where(id => id != Guid.Empty && _subscribedBoardIds.Contains(id))
            .Distinct()
            .ToList();

        if (toRemove.Count == 0 || _connection is null)
            return;

        try
        {
            await _connection.InvokeAsync("UnsubscribeAsync", (object)toRemove, ct);

            foreach (var id in toRemove)
                _subscribedBoardIds.Remove(id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Board export hub unsubscribe failed for {Count} board(s).", toRemove.Count);
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        _subscribedBoardIds.Clear();

        if (_connection is { } connection)
        {
            _connection = null;
            await connection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _connectGate.Dispose();

        if (_connection is { } connection)
            await connection.DisposeAsync();
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_connection?.State == HubConnectionState.Connected)
            return;

        await _connectGate.WaitAsync(ct);
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
            _connectGate.Release();
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

        var currentIds = _subscribedBoardIds.ToList();

        if (currentIds.Count == 0)
            return;

        try
        {
            await _connection!.InvokeAsync("SubscribeAsync", (object)currentIds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to re-subscribe after board export hub reconnect.");
        }
    }
}
