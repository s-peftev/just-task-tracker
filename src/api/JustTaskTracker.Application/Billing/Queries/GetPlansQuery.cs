using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Common.Results;
using MediatR;

namespace JustTaskTracker.Application.Billing.Queries;

public record GetPlansQuery : IRequest<Result<IReadOnlyList<PlanCardDto>>>;

public class GetPlansQueryHandler(
    IPlanCatalog planCatalog,
    IBillingService billingService)
    : IRequestHandler<GetPlansQuery, Result<IReadOnlyList<PlanCardDto>>>
{
    public async Task<Result<IReadOnlyList<PlanCardDto>>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        var plans = planCatalog.GetAllPlans();

        var cards = await Task.WhenAll(plans.Select(plan => MapToCardAsync(plan, ct)));

        return Result<IReadOnlyList<PlanCardDto>>.Success(cards);
    }

    private async Task<PlanCardDto> MapToCardAsync(PlanDto plan, CancellationToken ct)
    {
        PlanPriceDto? price = null;
        var priceId = planCatalog.TryGetPriceId(plan.PlanId);

        if (priceId is not null)
            price = await billingService.GetPriceAsync(priceId, ct);

        return new PlanCardDto(
            plan.PlanId,
            plan.PlanDisplayName,
            plan.Features,
            price);
    }
}
