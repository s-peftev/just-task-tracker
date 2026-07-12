using JustTaskTracker.WebUI.Domain.Billing;

namespace JustTaskTracker.WebUI.Services.Abstractions.Billing;

public interface IBillingApiService
{
    Task<PlanDto> GetEntitlementsAsync(CancellationToken ct = default);
}
