using JustTaskTracker.Application.Assistant.Repositories;
using JustTaskTracker.Domain.Assistant.ReadModels;

namespace JustTaskTracker.Application.Assistant.Tools.Handlers;

internal class GetRequesterAccountToolHandler(IAssistantDataQueryRepository assistantDataQueryRepository)
    : IAssistantToolHandler
{
    public string ToolName => AssistantToolNames.GetRequesterAccount;

    public async Task<string> ExecuteAsync(Guid currentUserId, CancellationToken ct = default)
    {
        var account = await assistantDataQueryRepository.GetUserRolesAndSubscriptionInfoAsync(currentUserId, ct);

        if (account is null)
            return AssistantToolJson.Error("Requester account was not found.");

        return AssistantToolJson.Serialize(ToPayload(account));
    }

    private static object ToPayload(AssistantRequesterAccountReadModel account)
    {
        var globalRoles = string.IsNullOrWhiteSpace(account.GlobalRoles)
            ? Array.Empty<string>()
            : account.GlobalRoles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new
        {
            globalRoles,
            subscription = new
            {
                hasBillableSubscription = account.HasBillableSubscription,
                planId = account.PlanId,
                status = account.SubscriptionStatus,
                cancelAtPeriodEnd = account.CancelAtPeriodEnd,
                currentPeriodStartUtc = account.CurrentPeriodStartUtc,
                currentPeriodEndUtc = account.CurrentPeriodEndUtc
            }
        };
    }
}
