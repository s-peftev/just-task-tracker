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

    public void Add(CallSessionLinkedTask linkedTask) =>
        context.CallSessionLinkedTasks.Add(linkedTask);
}
