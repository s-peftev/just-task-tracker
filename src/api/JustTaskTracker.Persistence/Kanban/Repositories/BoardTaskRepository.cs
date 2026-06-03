using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Kanban.Entities;
using JustTaskTracker.Persistence.Common;

namespace JustTaskTracker.Persistence.Kanban.Repositories;

public class BoardTaskRepository(JustTaskTrackerDbContext context)
    : Repository<BoardTask, Guid>(context), IBoardTaskRepository;
