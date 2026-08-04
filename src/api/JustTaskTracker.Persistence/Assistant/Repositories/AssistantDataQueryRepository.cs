using JustTaskTracker.Application.Assistant.Repositories;
using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Assistant.Repositories;

public class AssistantDataQueryRepository(JustTaskTrackerDbContext context) : IAssistantDataQueryRepository
{
    public Task<int> GetActiveOwnedBoardsCountAsync(Guid userId, CancellationToken ct = default) =>
        context.AssistantActiveOwnedBoardsCounts
            .Where(row => row.UserId == userId)
            .Select(row => row.ActiveOwnedBoardsCount)
            .FirstOrDefaultAsync(ct);

    public Task<AssistantRequesterAccountReadModel?> GetUserRolesAndSubscriptionInfoAsync(
        Guid userId,
        CancellationToken ct = default) =>
        context.AssistantRequesterAccounts
            .FirstOrDefaultAsync(row => row.UserId == userId, ct);
}
