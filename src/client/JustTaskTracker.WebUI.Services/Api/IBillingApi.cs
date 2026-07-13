using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Domain.Billing.Requests;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IBillingApi
{
    [Get("/api/billing/entitlements")]
    Task<IApiResponse<ApiEnvelope<PlanDto>>> GetEntitlementsAsync(CancellationToken ct = default);

    [Get("/api/billing/plans")]
    Task<IApiResponse<ApiEnvelope<IReadOnlyList<PlanCardDto>>>> GetPlansAsync(CancellationToken ct = default);

    [Get("/api/billing/subscription")]
    Task<IApiResponse<ApiEnvelope<SubscriptionDetailsDto>>> GetSubscriptionAsync(CancellationToken ct = default);

    [Post("/api/billing/checkout")]
    Task<IApiResponse<ApiEnvelope<CheckoutSessionResult>>> CreateCheckoutSessionAsync(
        [Body] CreateCheckoutSessionRequest request,
        CancellationToken ct = default);
}
