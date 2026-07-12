using JustTaskTracker.WebUI.Domain.Billing;

namespace JustTaskTracker.WebUI.Services.Abstractions.Billing;

public interface IBillingApiService
{
    Task<PlanDto> GetEntitlementsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PlanCardDto>> GetPlansAsync(CancellationToken ct = default);
}
