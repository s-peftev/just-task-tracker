using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Api.Models;
using Refit;

namespace JustTaskTracker.WebUI.Services.Api;

internal interface IBillingApi
{
    [Get("/api/billing/entitlements")]
    Task<IApiResponse<ApiEnvelope<PlanDto>>> GetEntitlementsAsync(CancellationToken ct = default);
}
