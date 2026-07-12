using JustTaskTracker.WebUI.Domain.Billing;
using JustTaskTracker.WebUI.Services.Abstractions.Billing;
using JustTaskTracker.WebUI.Services.Api;

namespace JustTaskTracker.WebUI.Services.Billing;

internal sealed class BillingApiService(IBillingApi api) : IBillingApiService
{
    public async Task<PlanDto> GetEntitlementsAsync(CancellationToken ct = default)
    {
        var response = await api.GetEntitlementsAsync(ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<IReadOnlyList<PlanCardDto>> GetPlansAsync(CancellationToken ct = default)
    {
        var response = await api.GetPlansAsync(ct);

        return ApiResponseGuard.Unwrap(response);
    }

    public async Task<SubscriptionDetailsDto> GetSubscriptionAsync(CancellationToken ct = default)
    {
        var response = await api.GetSubscriptionAsync(ct);

        return ApiResponseGuard.Unwrap(response);
    }
}
