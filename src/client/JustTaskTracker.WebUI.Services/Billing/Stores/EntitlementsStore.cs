using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Abstractions.Billing;

namespace JustTaskTracker.WebUI.Services.Billing.Stores;

/// <summary>
/// Scoped store for the authenticated user's plan entitlements.
/// Mirrors <see cref="Auth.Stores.ProfileStore"/> single-flight loading.
/// </summary>
internal sealed class EntitlementsStore(IBillingApiService billingApiService) : IEntitlementsStore, IDisposable
{
    private readonly SemaphoreSlim _sync = new(1, 1);
    private readonly HashSet<string> _features = new(StringComparer.Ordinal);

    private Task? _inFlightLoad;
    private int _loadGeneration;

    public PlanDto? Entitlements { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public Task EnsureLoadedAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: false, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, ct);

    public void Reset()
    {
        _loadGeneration++;
        Entitlements = null;
        _features.Clear();
        IsLoading = false;
        IsLoaded = false;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    public bool Can(string feature) =>
        !string.IsNullOrEmpty(feature) && _features.Contains(feature);

    public void Dispose() => _sync.Dispose();

    private async Task EnsureLoadedInternalAsync(bool forceRefresh, CancellationToken ct)
    {
        if (!forceRefresh && IsLoaded)
            return;

        Task loadTask;
        var clearedForRefresh = false;

        await _sync.WaitAsync(ct);
        try
        {
            if (!forceRefresh && IsLoaded)
                return;

            if (forceRefresh)
            {
                _loadGeneration++;
                Entitlements = null;
                _features.Clear();
                IsLoaded = false;
                clearedForRefresh = true;
            }

            if (forceRefresh || _inFlightLoad is null || _inFlightLoad.IsCompleted)
                _inFlightLoad = LoadEntitlementsAsync(_loadGeneration);

            loadTask = _inFlightLoad;
        }
        finally
        {
            _sync.Release();
        }

        if (clearedForRefresh)
            NotifyStateChanged();

        await loadTask.WaitAsync(ct);
    }

    private async Task LoadEntitlementsAsync(int generation)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var plan = await billingApiService.GetEntitlementsAsync(CancellationToken.None);

            if (IsStaleGeneration(generation))
                return;

            Entitlements = plan;
            _features.Clear();
            foreach (var feature in plan.Features)
                _features.Add(feature);

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            if (IsStaleGeneration(generation))
                return;

            Entitlements = null;
            _features.Clear();
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
