using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Persistence.Common;

namespace JustTaskTracker.Persistence.Boards.Repositories;

public class ColumnRepository(JustTaskTrackerDbContext context)
    : Repository<Column, Guid>(context), IColumnRepository;
