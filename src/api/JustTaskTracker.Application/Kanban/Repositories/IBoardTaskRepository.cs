using JustTaskTracker.Application.Common.Interfaces.Persistence;
using JustTaskTracker.Domain.Kanban.Entities;

namespace JustTaskTracker.Application.Kanban.Repositories;

public interface IBoardTaskRepository : IRepository<BoardTask, Guid>;
