using JustTaskTracker.Application.Kanban.Repositories;
using JustTaskTracker.Domain.Kanban.Entities;
using JustTaskTracker.Persistence.Common;

namespace JustTaskTracker.Persistence.Kanban.Repositories;

public class ColumnRepository(JustTaskTrackerDbContext context)
    : Repository<Column, Guid>(context), IColumnRepository;
