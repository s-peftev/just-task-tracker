using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Abstractions.Billing;

namespace JustTaskTracker.WebUI.Services.Billing.Stores;

/// <summary>
/// Page-scoped billing store (subscription + plans). Resolve from
/// <see cref="Microsoft.AspNetCore.Components.OwningComponentBase"/> so it lives
/// only while Subscriptions is open.
/// </summary>
internal sealed class BillingStore(IBillingApiService billingApiService) : IBillingStore, IDisposable
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private Task? _inFlightLoad;
    private int _loadGeneration;

    public IReadOnlyList<PlanCardDto> Plans { get; private set; } = [];
    public SubscriptionDetailsDto? Subscription { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task EnsureLoadedAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: false, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, ct);

    public void Dispose() => _sync.Dispose();

    private async Task EnsureLoadedInternalAsync(bool forceRefresh, CancellationToken ct)
    {
        if (!forceRefresh && IsLoaded)
            return;

        Task loadTask;

        await _sync.WaitAsync(ct);
        try
        {
            if (!forceRefresh && IsLoaded)
                return;

            if (forceRefresh)
            {
                _loadGeneration++;
                Plans = [];
                Subscription = null;
                IsLoaded = false;
            }

            if (forceRefresh || _inFlightLoad is null || _inFlightLoad.IsCompleted)
                _inFlightLoad = LoadAsync(_loadGeneration);

            loadTask = _inFlightLoad;
        }
        finally
        {
            _sync.Release();
        }

        await loadTask.WaitAsync(ct);
    }

    private async Task LoadAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            // Subscription first so plan cards can bind current-plan state immediately after catalog load.
            var subscription = await billingApiService.GetSubscriptionAsync(CancellationToken.None);
            var plans = await billingApiService.GetPlansAsync(CancellationToken.None);

            if (IsStaleGeneration(generation))
                return;

            Subscription = subscription;
            Plans = plans;
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

            Subscription = null;
            Plans = [];
            IsLoaded = false;
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (!IsStaleGeneration(generation))
                IsLoading = false;

            await _sync.WaitAsync(CancellationToken.None);
            try
            {
                if (!IsStaleGeneration(generation))
                    _inFlightLoad = null;
            }
            finally
            {
                _sync.Release();
            }

            NotifyStateChanged();
        }
    }

    private bool IsStaleGeneration(int generation) => generation != _loadGeneration;

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
