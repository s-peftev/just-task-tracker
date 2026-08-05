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

    public Task<AssistantRequesterAccountReadModel?> GetUserRolesAndSubscriptionInfoAsync(Guid userId, CancellationToken ct = default) =>
        context.AssistantRequesterAccounts
            .FirstOrDefaultAsync(row => row.UserId == userId, ct);

    public async Task<IReadOnlyList<AssistantMyBoardReadModel>> GetMyBoardsAsync(Guid userId, CancellationToken ct = default) =>
        await context.AssistantMyBoards
            .Where(row => row.UserId == userId)
            .OrderBy(row => row.BoardName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AssistantAssignedTaskReadModel>> GetAssignedTasksAsync(Guid userId, Guid boardId, CancellationToken ct = default) =>
        await context.AssistantAssignedTasks
            .Where(row => row.UserId == userId && row.BoardId == boardId)
            .OrderBy(row => row.CreatedAtUtc)
            .ToListAsync(ct);

    public Task<AssistantBoardContextReadModel?> GetBoardContextAsync(Guid userId, Guid boardId, CancellationToken ct = default) =>
        context.AssistantBoardContexts
            .FirstOrDefaultAsync(row => row.UserId == userId && row.BoardId == boardId, ct);
}
