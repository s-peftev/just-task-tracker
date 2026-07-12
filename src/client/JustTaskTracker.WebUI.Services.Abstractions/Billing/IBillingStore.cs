using JustTaskTracker.WebUI.Domain.Billing;

namespace JustTaskTracker.WebUI.Services.Abstractions.Billing;

/// <summary>
/// Page-scoped store for the Subscriptions screen. Resolve via
/// <c>OwningComponentBase</c> so the instance lives only while the page is open.
/// </summary>
public interface IBillingStore
{
    IReadOnlyList<PlanCardDto> Plans { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    string? ErrorMessage { get; }

    event Action? StateChanged;

    Task EnsurePlansLoadedAsync(CancellationToken ct = default);

    Task RefreshPlansAsync(CancellationToken ct = default);
}
