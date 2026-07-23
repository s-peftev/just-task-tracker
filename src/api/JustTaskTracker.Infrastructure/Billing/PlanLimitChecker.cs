using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Enums;
using JustTaskTracker.Domain.Billing.Errors;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;

namespace JustTaskTracker.Infrastructure.Billing;

internal class PlanLimitChecker(
    IEntitlementService entitlementService,
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardTaskRepository boardTaskRepository) : IPlanLimitChecker
{
    public async Task<Error?> EvaluateAsync(
        PlanLimitKind limit,
        Guid actorUserId,
        Guid? boardId,
        CancellationToken ct = default)
    {
        return limit switch
        {
            PlanLimitKind.Boards => await EvaluateBoardsAsync(actorUserId, ct),
            PlanLimitKind.ColumnsPerBoard
                or PlanLimitKind.TasksPerBoard
                or PlanLimitKind.MembersPerBoard
                => await EvaluateBoardScopedAsync(limit, boardId, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(limit), limit, "Unknown plan limit kind.")
        };
    }

    private async Task<Error?> EvaluateBoardsAsync(Guid actorUserId, CancellationToken ct)
    {
        var entitlements = await entitlementService.GetEntitlementsAsync(actorUserId, ct);
        var max = GetMax(entitlements.Limits, PlanLimitKind.Boards);

        if (max is null)
            return null;

        var count = await boardRepository.CountActiveOwnedBoardsByUserIdAsync(actorUserId, ct);

        return count >= max.Value ? EntitlementErrors.LimitReached : null;
    }

    private async Task<Error?> EvaluateBoardScopedAsync(
        PlanLimitKind limit,
        Guid? boardId,
        CancellationToken ct)
    {
        if (boardId is null)
        {
            throw new InvalidOperationException(
                $"Plan limit '{limit}' requires a board id.");
        }

        var ownerUserId = await boardRepository.GetOwnerUserIdAsync(boardId.Value, ct);

        if (ownerUserId is null)
            return GeneralErrors.NotFound;

        var entitlements = await entitlementService.GetEntitlementsAsync(ownerUserId.Value, ct);
        var max = GetMax(entitlements.Limits, limit);

        if (max is null)
            return null;

        var count = await GetBoardScopedCountAsync(limit, boardId.Value, ct);

        return count >= max.Value ? EntitlementErrors.BoardLimitReached : null;
    }

    private async Task<int> GetBoardScopedCountAsync(PlanLimitKind limit, Guid boardId, CancellationToken ct) =>
        limit switch
        {
            PlanLimitKind.ColumnsPerBoard => await columnRepository.CountByBoardIdAsync(boardId, ct),
            PlanLimitKind.TasksPerBoard => await boardTaskRepository.CountByBoardIdAsync(boardId, ct),
            PlanLimitKind.MembersPerBoard => await boardRepository.CountMembersByBoardIdAsync(boardId, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(limit), limit, "Not a board-scoped plan limit.")
        };

    private static int? GetMax(PlanLimitsDto limits, PlanLimitKind limit) =>
        limit switch
        {
            PlanLimitKind.Boards => limits.MaxBoards,
            PlanLimitKind.ColumnsPerBoard => limits.MaxColumnsPerBoard,
            PlanLimitKind.TasksPerBoard => limits.MaxTasksPerBoard,
            PlanLimitKind.MembersPerBoard => limits.MaxMembersPerBoard,
            _ => throw new ArgumentOutOfRangeException(nameof(limit), limit, "Unknown plan limit kind.")
        };
}
