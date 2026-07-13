using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Abstractions.Billing;

namespace JustTaskTracker.WebUI.Services.Billing.Stores;

/// <summary>
/// Scoped store for the authenticated user's plan entitlements.
/// Mirrors <see cref="Auth.Stores.ProfileStore"/> single-flight loading.
/// </summary>
internal sealed class EntitlementsStore(IBillingApiService billingApiService) : IEntitlementsStore, IDisposable
{
    private const int MaxPaymentConfirmationRetries = 5;
    private static readonly TimeSpan PaymentConfirmationRetryDelay = TimeSpan.FromSeconds(2);

    private readonly SemaphoreSlim _sync = new(1, 1);
    private readonly HashSet<string> _features = new(StringComparer.Ordinal);

    private Task? _inFlightLoad;
    private int _loadGeneration;
    private CancellationTokenSource? _paymentConfirmationCts;

    public PlanDto? Entitlements { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ExpectedPurchasedPlanId { get; private set; }
    public PaymentConfirmationStatus PaymentConfirmationStatus { get; private set; }

    public event Action? StateChanged;

    public Task EnsureLoadedAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: false, ct);

    public Task RefreshAsync(CancellationToken ct = default) =>
        EnsureLoadedInternalAsync(forceRefresh: true, ct);

    public async Task ConfirmPurchasedPlanAsync(string planId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        CancelPaymentConfirmation();
        _paymentConfirmationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var confirmationCt = _paymentConfirmationCts.Token;

        ExpectedPurchasedPlanId = planId.Trim();
        PaymentConfirmationStatus = PaymentConfirmationStatus.Confirming;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            if (await TryMatchPurchasedPlanAsync(ExpectedPurchasedPlanId, confirmationCt))
            {
                PaymentConfirmationStatus = PaymentConfirmationStatus.Confirmed;
                return;
            }

            for (var retry = 0; retry < MaxPaymentConfirmationRetries; retry++)
            {
                await Task.Delay(PaymentConfirmationRetryDelay, confirmationCt);

                if (await TryMatchPurchasedPlanAsync(ExpectedPurchasedPlanId, confirmationCt))
                {
                    PaymentConfirmationStatus = PaymentConfirmationStatus.Confirmed;
                    return;
                }
            }

            PaymentConfirmationStatus = PaymentConfirmationStatus.AwaitingActivation;
        }
        catch (OperationCanceledException) when (confirmationCt.IsCancellationRequested)
        {
            if (PaymentConfirmationStatus == PaymentConfirmationStatus.Confirming)
                PaymentConfirmationStatus = PaymentConfirmationStatus.Idle;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            PaymentConfirmationStatus = PaymentConfirmationStatus.AwaitingActivation;
        }
        finally
        {
            NotifyStateChanged();
        }
    }

    public void Reset()
    {
        CancelPaymentConfirmation();
        _loadGeneration++;
        Entitlements = null;
        _features.Clear();
        IsLoading = false;
        IsLoaded = false;
        ErrorMessage = null;
        ExpectedPurchasedPlanId = null;
        PaymentConfirmationStatus = PaymentConfirmationStatus.Idle;
        NotifyStateChanged();
    }

    public bool Can(string feature) =>
        !string.IsNullOrEmpty(feature) && _features.Contains(feature);

    public void Dispose()
    {
        CancelPaymentConfirmation();
        _sync.Dispose();
    }

    private async Task<bool> TryMatchPurchasedPlanAsync(string expectedPlanId, CancellationToken ct)
    {
        // Avoid RefreshAsync's clear-to-null so sidebar entitlements do not flicker during polls.
        var plan = await billingApiService.GetEntitlementsAsync(ct);

        await _sync.WaitAsync(ct);
        try
        {
            ApplyPlan(plan);
        }
        finally
        {
            _sync.Release();
        }

        NotifyStateChanged();

        return plan.PlanId.Equals(expectedPlanId, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyPlan(PlanDto plan)
    {
        Entitlements = plan;
        _features.Clear();
        foreach (var feature in plan.Features)
            _features.Add(feature);

        IsLoaded = true;
        IsLoading = false;
        ErrorMessage = null;
    }

    private void CancelPaymentConfirmation()
    {
        if (_paymentConfirmationCts is null)
            return;

        _paymentConfirmationCts.Cancel();
        _paymentConfirmationCts.Dispose();
        _paymentConfirmationCts = null;
    }

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

            await _sync.WaitAsync(CancellationToken.None);
            try
            {
                if (IsStaleGeneration(generation))
                    return;

                ApplyPlan(plan);
            }
            finally
            {
                _sync.Release();
            }
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
