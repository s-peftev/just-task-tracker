using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Abstractions.Billing;

namespace JustTaskTracker.WebUI.Services.Billing.Stores;

/// <summary>
/// Page-scoped billing store (plans catalog). Intended to be resolved from an
/// <see cref="Microsoft.AspNetCore.Components.OwningComponentBase"/> scope so it
/// is created on entering Subscriptions and disposed on leaving.
/// </summary>
internal sealed class BillingStore(IBillingApiService billingApiService) : IBillingStore, IDisposable
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private Task? _inFlightLoad;
    private int _loadGeneration;

    public IReadOnlyList<PlanCardDto> Plans { get; private set; } = [];
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task EnsurePlansLoadedAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: false, ct);

    public Task RefreshPlansAsync(CancellationToken ct = default) =>
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
                IsLoaded = false;
            }

            if (forceRefresh || _inFlightLoad is null || _inFlightLoad.IsCompleted)
                _inFlightLoad = LoadPlansAsync(_loadGeneration);

            loadTask = _inFlightLoad;
        }
        finally
        {
            _sync.Release();
        }

        await loadTask.WaitAsync(ct);
    }

    private async Task LoadPlansAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var plans = await billingApiService.GetPlansAsync(CancellationToken.None);

            if (IsStaleGeneration(generation))
                return;

            Plans = plans;
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

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
