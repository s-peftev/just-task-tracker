using JustTaskTracker.Domain.Billing.DTOs;

namespace JustTaskTracker.Application.Billing.Abstractions;

/// <summary>
/// Resolves effective billing entitlements for a user based on global app roles,
/// the plan catalog, and persisted subscription state.
/// </summary>
public interface IEntitlementService
{
    /// <summary>
    /// Determines whether the user is entitled to use <paramref name="feature"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Domain.Auth.Constants.Roles.Admin"/> receives all registered features.
    /// <see cref="Domain.Auth.Constants.Roles.Guest"/> receives no billing features.
    /// <see cref="Domain.Auth.Constants.Roles.User"/> receives features from the effective plan.
    /// </remarks>
    Task<bool> CanUseAsync(Guid userId, IReadOnlyList<string> globalRoles, string feature, CancellationToken ct = default);

    /// <summary>
    /// Returns the user's entitlements for API/UI consumption.
    /// </summary>
    Task<EntitlementDto> GetEntitlementsAsync(Guid userId, IReadOnlyList<string> globalRoles, CancellationToken ct = default);

    /// <summary>
    /// Returns subscription details for the billing UI. When the user has no
    /// persisted subscription, returns the catalog default plan with
    /// <see cref="SubscriptionDetailsDto.HasBillableSubscription"/> set to
    /// <see langword="false"/>.
    /// </summary>
    Task<SubscriptionDetailsDto> GetUserSubscriptionAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the Stripe customer id for the user's current billable subscription,
    /// or <see langword="null"/> when none exists.
    /// </summary>
    Task<string?> GetBillableStripeCustomerIdAsync(Guid userId, CancellationToken ct = default);
}
