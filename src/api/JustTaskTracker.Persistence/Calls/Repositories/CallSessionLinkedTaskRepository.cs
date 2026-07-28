using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallSessionLinkedTaskRepository(JustTaskTrackerDbContext context) : ICallSessionLinkedTaskRepository
{
    public async Task<IReadOnlyList<Guid>> GetLinkedTaskIdsAsync(Guid callSessionId, CancellationToken ct = default) =>
        await context.CallSessionLinkedTasks
            .Where(t => t.CallSessionId == callSessionId)
            .Select(t => t.TaskId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BoardTaskLookupDto>> GetLinkedTaskLookupsAsync(Guid callSessionId, CancellationToken ct = default) =>
        await context.CallSessionLinkedTasks
            .Where(link => link.CallSessionId == callSessionId)
            .Join(
                context.BoardTasks,
                link => link.TaskId,
                task => task.Id,
                (link, task) => new BoardTaskLookupDto(task.Id, task.ColumnId, task.Title, task.Description))
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<BoardTaskLookupDto>>> GetLinkedTaskLookupsForSessionsAsync(IReadOnlyList<Guid> callSessionIds, CancellationToken ct = default)
    {
        if (callSessionIds.Count is 0)
            return new Dictionary<Guid, IReadOnlyList<BoardTaskLookupDto>>();

        var links = await context.CallSessionLinkedTasks
            .Where(link => callSessionIds.Contains(link.CallSessionId))
            .Join(
                context.BoardTasks,
                link => link.TaskId,
                task => task.Id,
                (link, task) => new { link.CallSessionId, Lookup = new BoardTaskLookupDto(task.Id, task.ColumnId, task.Title, task.Description) })
            .ToListAsync(ct);

        return links
            .GroupBy(x => x.CallSessionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<BoardTaskLookupDto>)g.Select(x => x.Lookup).ToList());
    }

    public void Add(CallSessionLinkedTask linkedTask) =>
        context.CallSessionLinkedTasks.Add(linkedTask);
}
