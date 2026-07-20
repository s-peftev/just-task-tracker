using JustTaskTracker.WebUI.Domain.Billing;

namespace JustTaskTracker.WebUI.Services.Abstractions.Billing;

public interface IEntitlementsStore
{
    EntitlementDto? Entitlements { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }

    /// <summary>
    /// Plan id expected after Stripe Checkout redirect (<c>?planId=</c>).
    /// </summary>
    string? ExpectedPurchasedPlanId { get; }

    /// <summary>
    /// Outcome of <see cref="ConfirmPurchasedPlanAsync"/>.
    /// </summary>
    PaymentConfirmationStatus PaymentConfirmationStatus { get; }

    event Action? StateChanged;

    /// <summary>
    /// Ensures entitlements are loaded. No-ops if already loaded; attaches to
    /// the in-flight load if one is running; starts a new load otherwise.
    /// </summary>
    Task EnsureLoadedAsync(CancellationToken ct = default);

    /// <summary>
    /// Forces a fresh entitlements load regardless of current state.
    /// </summary>
    Task RefreshAsync(CancellationToken ct = default);

    /// <summary>
    /// After Stripe Checkout success redirect: polls entitlements until the
    /// effective plan matches <paramref name="planId"/> (or retries are exhausted).
    /// Updates cached entitlements on each attempt.
    /// </summary>
    Task ConfirmPurchasedPlanAsync(string planId, CancellationToken ct = default);

    /// <summary>
    /// Clears cached entitlements. Call on logout before MSAL redirect.
    /// </summary>
    void Reset();

    /// <summary>
    /// Returns whether the current plan includes <paramref name="feature"/>.
    /// Returns <see langword="false"/> until entitlements are loaded.
    /// </summary>
    bool Can(string feature);
}
