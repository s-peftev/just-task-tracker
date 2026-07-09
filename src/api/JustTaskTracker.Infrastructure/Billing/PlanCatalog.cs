using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Domain.Billing.DTOs;

namespace JustTaskTracker.Infrastructure.Billing;

internal class PlanCatalog(BillingOptions billingOptions) : IPlanCatalog
{
    public string DefaultPlanId => billingOptions.DefaultPlanId;

    public PlanDto GetPlan(string planId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        if (billingOptions.Plans is null
            || !billingOptions.Plans.TryGetValue(planId, out var plan))
        {
            throw new InvalidOperationException($"Billing plan '{planId}' is not defined in configuration.");
        }

        return ToPlanDto(plan);
    }

    public IReadOnlyList<PlanDto> GetAllPlans()
    {
        if (billingOptions.Plans is null || billingOptions.Plans.Count == 0)
            return [];

        return billingOptions.Plans
            .Values
            .Select(ToPlanDto)
            .ToList();
    }

    private static PlanDto ToPlanDto(PlanDefinitionOptions plan) =>
        new(plan.Id, plan.DisplayName, plan.Features);
}
