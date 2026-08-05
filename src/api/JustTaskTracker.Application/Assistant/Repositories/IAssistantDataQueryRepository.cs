using JustTaskTracker.Domain.Assistant.ReadModels;

namespace JustTaskTracker.Application.Assistant.Repositories;

public interface IAssistantDataQueryRepository
{
    Task<int> GetActiveOwnedBoardsCountAsync(Guid userId, CancellationToken ct = default);

    Task<AssistantRequesterAccountReadModel?> GetUserRolesAndSubscriptionInfoAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<AssistantMyActiveBoardReadModel>> GetMyActiveBoardsAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<AssistantAssignedTaskReadModel>> GetAssignedTasksAsync(Guid userId, Guid boardId, CancellationToken ct = default);
}
