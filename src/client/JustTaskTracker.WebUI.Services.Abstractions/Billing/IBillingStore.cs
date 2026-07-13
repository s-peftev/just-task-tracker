using JustTaskTracker.WebUI.Domain.Billing;

namespace JustTaskTracker.WebUI.Services.Abstractions.Billing;

/// <summary>
/// Page-scoped store for the Subscriptions screen. Resolve via
/// <c>OwningComponentBase</c> so the instance lives only while the page is open.
/// </summary>
public interface IBillingStore
{
    IReadOnlyList<PlanCardDto> Plans { get; }
    SubscriptionDetailsDto? Subscription { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    /// <summary>
    /// Loads current subscription first, then the plans catalog.
    /// </summary>
    Task EnsureLoadedAsync(CancellationToken ct = default);

    Task RefreshAsync(CancellationToken ct = default);
}
